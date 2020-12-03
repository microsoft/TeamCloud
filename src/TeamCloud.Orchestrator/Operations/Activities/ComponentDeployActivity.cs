/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Deployment;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Templates;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentDeployActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public ComponentDeployActivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new System.ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(ComponentDeployActivity))]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var template = new ComponentDeployTemplate();

            template.Parameters["ComponentId"] = input.Component.Id;

            var deployment = await azureDeploymentService
                .DeploySubscriptionTemplateAsync(template, Guid.Parse(input.Organization.SubscriptionId), input.Organization.Location)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }

        internal struct Input
        {
            public Organization Organization { get; set; }

            public Component Component { get; set; }
        }
    }
}
