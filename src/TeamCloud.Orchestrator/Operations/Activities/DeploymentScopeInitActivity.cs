/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Templates.ResourceGroup;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class DeploymentScopeInitAcitivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public DeploymentScopeInitAcitivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new System.ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(DeploymentScopeInitAcitivity))]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var template = new DeployementScopeInitTemplate();

            template.Parameters["deployementScopeId"] = input.DeploymentScope.Id;

            var projectResourceGroupId = AzureResourceIdentifier.Parse(input.Project.ResourceId);

            var deployment = await azureDeploymentService
                .DeployResourceGroupTemplateAsync(template, projectResourceGroupId.SubscriptionId, projectResourceGroupId.ResourceGroup)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }

        public struct Input
        {
            public Project Project { get; set; }

            public DeploymentScope DeploymentScope { get; set; }
        }
    }
}
