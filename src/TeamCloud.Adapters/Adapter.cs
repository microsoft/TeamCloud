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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Azure.Directory;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
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
        private readonly IUserRepository userRepository;

        protected Adapter(IAuthorizationSessionClient sessionClient,
                          IAuthorizationTokenClient tokenClient,
                          IDistributedLockManager distributedLockManager,
                          ISecretsStoreProvider secretsStoreProvider,
                          IAzureSessionService azureSessionService,
                          IAzureDirectoryService azureDirectoryService,
                          IOrganizationRepository organizationRepository,
                          IDeploymentScopeRepository deploymentScopeRepository,
                          IProjectRepository projectRepository,
                          IUserRepository userRepository)
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
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

        public Task<Component> CreateComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (contextUser is null)
                throw new ArgumentNullException(nameof(contextUser));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            return ExecuteAsync(component, async (componentOrganization, componentDeploymentScope, componentProject) =>
            {
                if (this is IAdapterIdentity)
                {
                    await EnsureServiceIdentityUserAsync(component, contextUser, commandQueue)
                        .ConfigureAwait(false);
                }

                return await CreateComponentAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue)
                    .ConfigureAwait(false);
            });
        }

        protected abstract Task<Component> CreateComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue);

        public Task<Component> UpdateComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (contextUser is null)
                throw new ArgumentNullException(nameof(contextUser));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            return ExecuteAsync(component, async (componentOrganization, componentDeploymentScope, componentProject) =>
            {
                if (this is IAdapterIdentity)
                {
                    await EnsureServiceIdentityUserAsync(component, contextUser, commandQueue)
                        .ConfigureAwait(false);
                }

                return await UpdateComponentAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue)
                    .ConfigureAwait(false);
            });
        }

        protected abstract Task<Component> UpdateComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue);

        public Task<Component> DeleteComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (contextUser is null)
                throw new ArgumentNullException(nameof(contextUser));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            return ExecuteAsync(component, async (componentOrganization, componentDeploymentScope, componentProject) =>
            {
                if (this is IAdapterIdentity)
                {
                    await EnsureServiceIdentityUserAsync(component, contextUser, commandQueue)
                        .ConfigureAwait(false);
                }

                return await DeleteComponentAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue)
                    .ConfigureAwait(false);
            });
        }

        protected abstract Task<Component> DeleteComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue);

        private async Task EnsureServiceIdentityUserAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue)
        {
            var servicePrincipal = await GetServiceIdentityAsync(component)
                .ConfigureAwait(false);

            if (servicePrincipal != null)
            {
                await ExecuteAsync(component, async (componentOrganization, componentDeploymentScope, componentProject) =>
                {
                    var servicePrincipalUser = await userRepository
                        .GetAsync(component.Organization, servicePrincipal.ObjectId.ToString())
                        .ConfigureAwait(false);

                    if (servicePrincipalUser is null || servicePrincipalUser.ProjectMembership(componentProject.Id) is null)
                    {
                        var servicePrincipalUserIsNew = (servicePrincipalUser is null);

                        servicePrincipalUser ??= new User
                        {
                            Id = servicePrincipal.ObjectId.ToString(),
                            Role = OrganizationUserRole.None,
                            UserType = Model.Data.UserType.Service,
                            Organization = component.Organization
                        };

                        servicePrincipalUser.EnsureProjectMembership(componentProject.Id, ProjectUserRole.Adapter);

                        var projectUserCommand = servicePrincipalUserIsNew
                            ? (ICommand)new ProjectUserCreateCommand(contextUser, servicePrincipalUser, componentProject.Id)
                            : (ICommand)new ProjectUserUpdateCommand(contextUser, servicePrincipalUser, componentProject.Id);

                        await commandQueue
                            .AddAsync(projectUserCommand)
                            .ConfigureAwait(false);
                    }

                }).ConfigureAwait(false);
            }
        }

        protected Task<AzureServicePrincipal> GetServiceIdentityAsync(Component component, bool withPassword = false)
            => this is IAdapterIdentity ? ExecuteAsync(component, async (componentOrganization, componentDeploymentScope, componentProject) =>
        {
            var servicePrincipalKey = Guid.Parse(componentDeploymentScope.Organization)
                .Combine(Guid.Parse(componentDeploymentScope.Id), Guid.Parse(componentProject.Id));

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
                    .GetSecretsStoreAsync(componentProject)
                    .ConfigureAwait(false);

                servicePrincipal = await secretsStore
                    .SetSecretAsync(servicePrincipal.ObjectId.ToString(), servicePrincipal)
                    .ConfigureAwait(false);
            }
            else if (withPassword)
            {
                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(componentProject)
                    .ConfigureAwait(false);

                servicePrincipal = (await secretsStore
                    .GetSecretAsync<AzureServicePrincipal>(servicePrincipal.ObjectId.ToString())
                    .ConfigureAwait(false)) ?? servicePrincipal;
            }

            return servicePrincipal;

        }) : Task.FromResult(default(AzureServicePrincipal));

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
