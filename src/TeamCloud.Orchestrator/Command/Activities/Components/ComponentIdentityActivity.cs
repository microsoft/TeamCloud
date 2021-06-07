/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Activities.Components
{
    public sealed class ComponentIdentityActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentIdentityActivity(IOrganizationRepository organizationRepository,
                                         IDeploymentScopeRepository deploymentScopeRepository,
                                         IProjectRepository projectRepository,
                                         IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentIdentityActivity))]
        [RetryOptions(3)]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var component = context.GetInput<Input>().Component;

            if (!AzureResourceIdentifier.TryParse(component.IdentityId, out var identityId))
            {
                var deploymentScope = await deploymentScopeRepository
                    .GetAsync(component.Organization, component.DeploymentScopeId)
                    .ConfigureAwait(false);

                var project = await projectRepository
                    .GetAsync(component.Organization, component.ProjectId)
                    .ConfigureAwait(false);

                var projectResourceId = AzureResourceIdentifier.Parse(project.ResourceId);

                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync(projectResourceId.SubscriptionId)
                    .ConfigureAwait(false);

                var identities = await session.Identities
                    .ListByResourceGroupAsync(projectResourceId.ResourceGroup, loadAllPages: true)
                    .ConfigureAwait(false);

                var identity = identities
                    .SingleOrDefault(i => i.Name.Equals(deploymentScope.Id, StringComparison.OrdinalIgnoreCase));

                if (identity is null)
                {
                    var location = await GetLocationAsync(component)
                        .ConfigureAwait(false);

                    identity = await session.Identities
                        .Define(deploymentScope.Id)
                            .WithRegion(location)
                            .WithExistingResourceGroup(projectResourceId.ResourceGroup)
                        .CreateAsync()
                        .ConfigureAwait(false);
                }

                component.IdentityId = identity.Id;
            }


            return component;
        }

        private async Task<string> GetLocationAsync(Component component)
        {
            var tenantId = (await azureResourceService.AzureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            return organization.Location;
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
