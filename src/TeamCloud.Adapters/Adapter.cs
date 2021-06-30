/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Azure.Directory;
using TeamCloud.Data;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Secrets;

namespace TeamCloud.Adapters
{
    public abstract class Adapter : IAdapter
    {
        private static readonly JSchema dataSchemaEmpty = new JSchema() { Type = JSchemaType.Object };
        private static readonly JObject formSchemaEmpty = new JObject();

#pragma warning disable CS0618 // Type or member is obsolete

        // IDistributedLockManager is marked as obsolete, because it's not ready for "prime time"
        // however; it is used to managed singleton function execution within the functions fx !!!

        private readonly IAuthorizationSessionClient sessionClient;
        private readonly IAuthorizationTokenClient tokenClient;
        private readonly IDistributedLockManager distributedLockManager;
        private readonly ISecretsStoreProvider secretsStoreProvider;
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly IOrganizationRepository organizationRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IProjectRepository projectRepository;

        protected Adapter(IAuthorizationSessionClient sessionClient,
                          IAuthorizationTokenClient tokenClient,
                          IDistributedLockManager distributedLockManager,
                          ISecretsStoreProvider secretsStoreProvider,
                          IAzureSessionService azureSessionService,
                          IAzureDirectoryService azureDirectoryService,
                          IOrganizationRepository organizationRepository,
                          IDeploymentScopeRepository deploymentScopeRepository,
                          IProjectRepository projectRepository)
        {
            this.sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
            this.tokenClient = tokenClient ?? throw new ArgumentNullException(nameof(tokenClient));
            this.distributedLockManager = distributedLockManager ?? throw new ArgumentNullException(nameof(distributedLockManager));
            this.secretsStoreProvider = secretsStoreProvider ?? throw new ArgumentNullException(nameof(secretsStoreProvider));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

#pragma warning restore CS0618 // Type or member is obsolete

        public abstract DeploymentScopeType Type { get; }

        public abstract IEnumerable<ComponentType> ComponentTypes { get; }

        public virtual string DisplayName
            => Type.ToString(prettyPrint: true);

        protected IAuthorizationSessionClient SessionClient
            => sessionClient;

        protected IAuthorizationTokenClient TokenClient
            => tokenClient;

        protected Task<AdapterLock> AcquireLockAsync(string lockId, params string[] lockIdQualifiers)
        {
            if (string.IsNullOrWhiteSpace(lockId))
                throw new ArgumentException($"'{nameof(lockId)}' cannot be null or whitespace.", nameof(lockId));

            return AcquireLockAsync(string.Join('/', lockIdQualifiers.Prepend(lockId).Where(item => !string.IsNullOrWhiteSpace(item))));
        }

        protected async Task<AdapterLock> AcquireLockAsync(string lockId, int leaseTimeoutInSeconds = 60, int acquisitionTimeoutInSeconds = 60)
        {
            if (string.IsNullOrWhiteSpace(lockId))
                throw new ArgumentException($"'{nameof(lockId)}' cannot be null or whitespace.", nameof(lockId));

            leaseTimeoutInSeconds = Math.Max(leaseTimeoutInSeconds, 0);
            acquisitionTimeoutInSeconds = Math.Max(acquisitionTimeoutInSeconds, 0);

            var distributedLock = await distributedLockManager
                .TryLockAsync(null, lockId, null, null, TimeSpan.FromSeconds(leaseTimeoutInSeconds), CancellationToken.None)
                .ConfigureAwait(false);

            var acquisitionTimeoutElapsed = 0;

            while (distributedLock is null && acquisitionTimeoutElapsed++ < acquisitionTimeoutInSeconds)
            {
                await Task
                    .Delay(1000)
                    .ConfigureAwait(false);

                distributedLock = await distributedLockManager
                    .TryLockAsync(null, lockId, null, null, TimeSpan.FromSeconds(leaseTimeoutInSeconds), CancellationToken.None)
                    .ConfigureAwait(false);
            }

            if (distributedLock is null)
                throw new TimeoutException($"Failed to acquire lock for id '{lockId}' within {acquisitionTimeoutInSeconds} sec.");

            return new AdapterLock(distributedLockManager, distributedLock);
        }

        public virtual Task<string> GetInputDataSchemaAsync()
            => Task.FromResult(dataSchemaEmpty.ToString(Formatting.None));

        public virtual Task<string> GetInputFormSchemaAsync()
            => Task.FromResult(formSchemaEmpty.ToString(Formatting.None));

        public virtual Task<NetworkCredential> GetServiceCredentialAsync(Component component)
            => Task.FromResult(default(NetworkCredential));

        public abstract Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope);

        public abstract Task<Component> CreateComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log);

        public abstract Task<Component> UpdateComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log);

        public abstract Task<Component> DeleteComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log);

        protected async Task<AzureServicePrincipal> GetServicePrincipalAsync(DeploymentScope deploymentScope, Project project, bool withPassword = false)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (project is null)
                throw new ArgumentNullException(nameof(project));

            if (deploymentScope.Organization != project.Organization)
                throw new ArgumentException($"Deployment scope and project must belong to the same organization");

            var servicePrincipalKey = Guid.Parse(deploymentScope.Organization)
                .Combine(Guid.Parse(deploymentScope.Id), Guid.Parse(project.Id));

            var servicePrincipalName = $"{this.GetType().Name}/{servicePrincipalKey}";

            var servicePrincipal = await azureDirectoryService
                .GetServicePrincipalAsync(servicePrincipalName)
                .ConfigureAwait(false);

            if (servicePrincipal is null)
            {
                // there is no service principal for the current deployment scope
                // create a new one that we can use to create/update the corresponding
                // service endpoint in the current team project

                servicePrincipal = await azureDirectoryService
                    .CreateServicePrincipalAsync(servicePrincipalName)
                    .ConfigureAwait(false);
            }
            else if (servicePrincipal.ExpiresOn.GetValueOrDefault(DateTime.MinValue) < DateTime.UtcNow)
            {
                // a service principal exists, but its secret is expired. lets refresh
                // the service principal (create a new secret) so we can move on
                // creating/updating the corresponding service endpoint.

                servicePrincipal = await azureDirectoryService
                    .RefreshServicePrincipalAsync(servicePrincipalName)
                    .ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(servicePrincipal.Password))
            {
                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(project)
                    .ConfigureAwait(false);

                servicePrincipal = await secretsStore
                    .SetSecretAsync(servicePrincipal.ObjectId.ToString(), servicePrincipal)
                    .ConfigureAwait(false);
            }
            else if (withPassword)
            {
                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(project)
                    .ConfigureAwait(false);

                servicePrincipal = (await secretsStore
                    .GetSecretAsync<AzureServicePrincipal>(servicePrincipal.ObjectId.ToString())
                    .ConfigureAwait(false)) ?? servicePrincipal;
            }

            return servicePrincipal;
        }

        protected async Task ExecuteAsync(Component component, Func<Organization, DeploymentScope, Project, Task> callback)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var tenantId = await azureSessionService
                .GetTenantIdAsync()
                .ConfigureAwait(false);

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization, true)
                .ConfigureAwait(false);

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId)
                .ConfigureAwait(false);

            var project = await projectRepository
                .GetAsync(component.Organization, component.ProjectId)
                .ConfigureAwait(false);

            await callback(organization, deploymentScope, project).ConfigureAwait(false);
        }

        protected async Task<T> ExecuteAsync<T>(Component component, Func<Organization, DeploymentScope, Project, Task<T>> callback)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var tenantId = await azureSessionService
                .GetTenantIdAsync()
                .ConfigureAwait(false);

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization, true)
                .ConfigureAwait(false);

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId)
                .ConfigureAwait(false);

            var project = await projectRepository
                .GetAsync(component.Organization, component.ProjectId)
                .ConfigureAwait(false);

            return await callback(organization, deploymentScope, project).ConfigureAwait(false);
        }
    }
}
