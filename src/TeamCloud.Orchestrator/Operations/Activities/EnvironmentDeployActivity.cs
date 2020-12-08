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
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Templates.Subscription;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class EnvironmentDeployActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;
        private readonly IOrganizationRepository organizationRepository;
        private readonly IAzureSessionService azureSessionService;

        public EnvironmentDeployActivity(IAzureDeploymentService azureDeploymentService, IOrganizationRepository organizationRepository, IAzureSessionService azureSessionService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new System.ArgumentNullException(nameof(azureDeploymentService));
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(EnvironmentDeployActivity))]
        [RetryOptions(3)]
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

            var template = new EnvironmentDeployTemplate();

            template.Parameters["componentId"] = input.Component.Id;
            template.Parameters["componentSlug"] = input.Component.Slug;
            template.Parameters["identityId"] = input.Component.IdentityId;

            var deployment = await azureDeploymentService
                .DeploySubscriptionTemplateAsync(template, deployementSubscription, organization.Location)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }

        internal struct Input
        {
            public DeploymentScope DeploymentScope { get; set; }

            public Component Component { get; set; }
        }
    }
}
