/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Azure.Directory;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Secrets;

namespace TeamCloud.Adapters;

public abstract class AdapterWithIdentity : Adapter, IAdapterIdentity
{
    private readonly ISecretsStoreProvider secretsStoreProvider;
    private readonly IAzureSessionService azureSessionService;
    private readonly IAzureDirectoryService azureDirectoryService;
    private readonly IOrganizationRepository organizationRepository;
    private readonly IProjectRepository projectRepository;

#pragma warning disable CS0618 // Type or member is obsolete

    protected AdapterWithIdentity(IAuthorizationSessionClient sessionClient,
                                  IAuthorizationTokenClient tokenClient,
                                  IDistributedLockManager distributedLockManager,
                                  ISecretsStoreProvider secretsStoreProvider,
                                  IAzureSessionService azureSessionService,
                                  IAzureDirectoryService azureDirectoryService,
                                  IOrganizationRepository organizationRepository,
                                  IDeploymentScopeRepository deploymentScopeRepository,
                                  IProjectRepository projectRepository,
                                  IUserRepository userRepository) : base(sessionClient, tokenClient, distributedLockManager, secretsStoreProvider, azureSessionService, azureDirectoryService, organizationRepository, deploymentScopeRepository, projectRepository, userRepository)
    {
        this.secretsStoreProvider = secretsStoreProvider ?? throw new ArgumentNullException(nameof(secretsStoreProvider));
        this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
    }

#pragma warning restore CS0618 // Type or member is obsolete

    public virtual async Task<AzureServicePrincipal> GetServiceIdentityAsync(Component component, bool withPassword = false)
    {
        if (this is IAdapterIdentity)
        {
            var servicePrincipalKey = Guid.Parse(component.Organization)
                .Combine(Guid.Parse(component.DeploymentScopeId), Guid.Parse(component.ProjectId));

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
                var project = await projectRepository
                    .GetAsync(component.Organization, component.ProjectId)
                    .ConfigureAwait(false);

                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(project)
                    .ConfigureAwait(false);

                servicePrincipal = await secretsStore
                    .SetSecretAsync(servicePrincipal.ObjectId.ToString(), servicePrincipal)
                    .ConfigureAwait(false);
            }
            else if (withPassword)
            {
                var project = await projectRepository
                    .GetAsync(component.Organization, component.ProjectId)
                    .ConfigureAwait(false);

                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(project)
                    .ConfigureAwait(false);

                servicePrincipal = (await secretsStore
                    .GetSecretAsync<AzureServicePrincipal>(servicePrincipal.ObjectId.ToString())
                    .ConfigureAwait(false)) ?? servicePrincipal;
            }

            return servicePrincipal;
        }

        return null;
    }

    public virtual async Task<AzureServicePrincipal> GetServiceIdentityAsync(DeploymentScope deploymentScope, bool withPassword = false)
    {
        if (this is IAdapterIdentity)
        {
            var servicePrincipalKey = Guid.Parse(deploymentScope.Organization)
                .Combine(Guid.Parse(deploymentScope.Id));

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
                var tenantId = await azureSessionService
                    .GetTenantIdAsync()
                    .ConfigureAwait(false);

                var organization = await organizationRepository
                    .GetAsync(tenantId.ToString(), deploymentScope.Organization)
                    .ConfigureAwait(false);

                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(organization)
                    .ConfigureAwait(false);

                servicePrincipal = await secretsStore
                    .SetSecretAsync(servicePrincipal.ObjectId.ToString(), servicePrincipal)
                    .ConfigureAwait(false);
            }
            else if (withPassword)
            {
                var tenantId = await azureSessionService
                    .GetTenantIdAsync()
                    .ConfigureAwait(false);

                var organization = await organizationRepository
                    .GetAsync(tenantId.ToString(), deploymentScope.Organization)
                    .ConfigureAwait(false);

                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(organization)
                    .ConfigureAwait(false);

                servicePrincipal = (await secretsStore
                    .GetSecretAsync<AzureServicePrincipal>(servicePrincipal.ObjectId.ToString())
                    .ConfigureAwait(false)) ?? servicePrincipal;
            }

            return servicePrincipal;
        }

        return null;
    }

}
