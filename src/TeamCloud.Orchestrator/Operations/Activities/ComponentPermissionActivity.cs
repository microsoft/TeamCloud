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
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentPermissionActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IUserRepository userRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentPermissionActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IUserRepository userRepository, IDeploymentScopeRepository deploymentScopeRepository, IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

            try
            {
                var task = component.Type switch
                {
                    ComponentType.Environment => HandleEnvironmentAsync(component),
                    _ => Task.CompletedTask
                };

                await task.ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                throw exc.AsSerializable();
            }

            return component;
        }

        private async Task HandleEnvironmentAsync(Component component)
        {
            if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                var roleDefinitionId = AzureRoleDefinition.Contributor;

                var roleAssignmentMap = await userRepository
                    .ListAsync(component.Organization, component.ProjectId)
                    .ToDictionaryAsync(user => user.Id, user => Enumerable.Repeat(roleDefinitionId, 1))
                    .ConfigureAwait(false);

                if (AzureResourceIdentifier.TryParse(component.IdentityId, out var identityResourceId))
                {
                    var session = await azureResourceService.AzureSessionService
                        .CreateSessionAsync(identityResourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    var identity = await session.Identities
                        .GetByIdAsync(identityResourceId.ToString())
                        .ConfigureAwait(false);

                    roleAssignmentMap
                        .Add(identity.PrincipalId, Enumerable.Repeat(AzureRoleDefinition.Contributor, 1));
                }

                if (string.IsNullOrEmpty(componentResourceId.ResourceGroup))
                {
                    var subscription = await azureResourceService
                        .GetSubscriptionAsync(componentResourceId.SubscriptionId, throwIfNotExists: true)
                        .ConfigureAwait(false);

                    await subscription
                        .SetRoleAssignmentsAsync(roleAssignmentMap)
                        .ConfigureAwait(false);
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup, throwIfNotExists: true)
                        .ConfigureAwait(false);

                    await resourceGroup
                        .SetRoleAssignmentsAsync(roleAssignmentMap)
                        .ConfigureAwait(false);
                }
            }
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
