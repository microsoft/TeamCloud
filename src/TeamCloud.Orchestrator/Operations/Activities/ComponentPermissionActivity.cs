/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentPermissionActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentPermissionActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IDeploymentScopeRepository deploymentScopeRepository, IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentPermissionActivity))]
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

            if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                var identityResourceId = AzureResourceIdentifier.Parse(component.IdentityId);

                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync(identityResourceId.SubscriptionId)
                    .ConfigureAwait(false);

                var identity = await session.Identities
                    .GetByIdAsync(identityResourceId.ToString())
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(componentResourceId.ResourceGroup))
                {
                    var subscription = await azureResourceService
                        .GetSubscriptionAsync(componentResourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    var isContributor = await subscription
                        .HasRoleAssignmentAsync(identity.PrincipalId, AzureRoleDefinition.Contributor)
                        .ConfigureAwait(false);

                    if (!isContributor)
                    {
                        await subscription
                            .AddRoleAssignmentAsync(identity.PrincipalId, AzureRoleDefinition.Contributor)
                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup)
                        .ConfigureAwait(false);

                    var isContributor = await resourceGroup
                        .HasRoleAssignmentAsync(identity.PrincipalId, AzureRoleDefinition.Contributor)
                        .ConfigureAwait(false);

                    if (!isContributor)
                    {
                        await resourceGroup
                            .AddRoleAssignmentAsync(identity.PrincipalId, AzureRoleDefinition.Contributor)
                            .ConfigureAwait(false);
                    }
                }
            }

            return component;
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
