/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Templates;
using TeamCloud.Orchestrator.Templates.Subscription;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class EnvironmentInitActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;
        private readonly IOrganizationRepository organizationRepository;
        private readonly IAzureSessionService azureSessionService;

        public EnvironmentInitActivity(IAzureDeploymentService azureDeploymentService, IOrganizationRepository organizationRepository, IAzureSessionService azureSessionService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new System.ArgumentNullException(nameof(azureDeploymentService));
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(EnvironmentInitActivity))]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var organization = await organizationRepository
                .GetAsync(azureSessionService.Options.TenantId, input.DeploymentScope.Organization)
                .ConfigureAwait(false);

            var deployementSubscriptionIndex = DateTime.UtcNow.Ticks % input.DeploymentScope.SubscriptionIds.Count;
            var deployementSubscription = input.DeploymentScope.SubscriptionIds[(int)deployementSubscriptionIndex];

            var template = new EnvironmentInitTemplate();

            template.Parameters["componentId"] = input.Component.Id;
            template.Parameters["identityId"] = input.Component.IdentityId;

            var deployment = await azureDeploymentService
                .DeploySubscriptionTemplateAsync(template, deployementSubscription, organization.Location)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }

        public struct Input
        {
            public DeploymentScope DeploymentScope { get; set; }

            public Component Component { get; set; }
        }
    }
}
