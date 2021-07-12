using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
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
    public abstract class AdapterWithIdentity : Adapter, IAdapterIdentity
    {
        private readonly ISecretsStoreProvider secretsStoreProvider;
        private readonly IAzureDirectoryService azureDirectoryService;

        protected AdapterWithIdentity(
            IAuthorizationSessionClient sessionClient,
            IAuthorizationTokenClient tokenClient,
            IDistributedLockManager distributedLockManager,
            ISecretsStoreProvider secretsStoreProvider,
            IAzureSessionService azureSessionService,
            IAzureDirectoryService azureDirectoryService,
            IOrganizationRepository organizationRepository,
            IDeploymentScopeRepository deploymentScopeRepository,
            IProjectRepository projectRepository)
            : base(
                sessionClient,
                tokenClient,
                distributedLockManager,
                secretsStoreProvider,
                azureSessionService,
                azureDirectoryService,
                organizationRepository,
                deploymentScopeRepository,
                projectRepository)
        {
            this.secretsStoreProvider = secretsStoreProvider ?? throw new ArgumentNullException(nameof(secretsStoreProvider));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
        }

        public async Task<AzureServicePrincipal> GetIdentityAsync(DeploymentScope deploymentScope, Project project, bool withPassword = false)
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
                // and project combination - lets create a new one

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
                // the service principal already comes with a password.
                // so it's a brand new idenity or the password expired
                // and was refreshed. anyway - we need to update our
                // secrets store.

                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(project)
                    .ConfigureAwait(false);

                servicePrincipal = await secretsStore
                    .SetSecretAsync(servicePrincipal.ObjectId.ToString(), servicePrincipal)
                    .ConfigureAwait(false);
            }
            else if (withPassword)
            {
                // the service principal was resolved via the graph api
                // but without a password information. however, the caller
                // explicitly requested the identity incl. password. 
                // so lets use our secrets store to resolve the full service principal.

                var secretsStore = await secretsStoreProvider
                    .GetSecretsStoreAsync(project)
                    .ConfigureAwait(false);

                servicePrincipal = (await secretsStore
                    .GetSecretAsync<AzureServicePrincipal>(servicePrincipal.ObjectId.ToString())
                    .ConfigureAwait(false)) ?? servicePrincipal;
            }

            return servicePrincipal;
        }
    }
}
