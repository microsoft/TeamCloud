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
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Operations.Activities.Templates
{
    public sealed class ComponentCleanActivity
    {
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureDeploymentService azureDeploymentService;

        public ComponentCleanActivity(IAzureSessionService azureSessionService, IAzureDeploymentService azureDeploymentService)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(ComponentCleanActivity))]
        [RetryOptions(3)]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var component = context.GetInput<Input>().Component;

            if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                var template = new ComponentCleanTemplate();

                var deployment = await azureDeploymentService
                    .DeployResourceGroupTemplateAsync(template, componentResourceId.SubscriptionId, componentResourceId.ResourceGroup, true)
                    .ConfigureAwait(false);

                return deployment.ResourceId;
            }

            return null;
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
