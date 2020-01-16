/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Deployments;
using TeamCloud.Model.Context;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Templates;

namespace TeamCloud.Orchestrator.Activities
{
    public class AzureResourceGroupCreateActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public AzureResourceGroupCreateActivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(AzureResourceGroupCreateActivity))]
        public async Task<AzureResourceGroup> RunActivity(
            [ActivityTrigger] (OrchestratorContext, Project) input)
        {
            var orchestratorContext = input.Item1;

            if (orchestratorContext == null)
                throw new ArgumentNullException(nameof(input));

            var project = input.Item2;

            if (project == null)
                throw new ArgumentNullException(nameof(input));

            // get the subscription from project state if possible
            var subscriptionId = project.ResourceGroup?.SubscriptionId;

            if (subscriptionId.GetValueOrDefault(Guid.Empty) == Guid.Empty)
            {
                // TODO: Resolve subscription id from pool
                subscriptionId = Guid.Parse(orchestratorContext.TeamCloud.Configuration.Azure.SubscriptionId);
            }

            var template = new CreateProjectTemplate();

            template.Parameters["projectId"] = project.Id;
            template.Parameters["projectName"] = project.Name;
            template.Parameters["projectPrefix"] = orchestratorContext.TeamCloud.Configuration.Azure.ResourceGroupNamePrefix;

            template.Parameters["resourceGroupName"] = project.ResourceGroup?.ResourceGroupName; // if null - the template generates a unique name
            template.Parameters["resourceGroupLocation"] = project.ResourceGroup?.Region ?? orchestratorContext.TeamCloud.Configuration.Azure.Region;

            var deployment = await azureDeploymentService
                .DeployTemplateAsync(template, subscriptionId.Value)
                .ConfigureAwait(false);

            _ = await deployment
                .WaitAsync(throwOnError: true)
                .ConfigureAwait(false);

            var deploymentOutput = await deployment
                .GetOutputAsync()
                .ConfigureAwait(false);

            return new AzureResourceGroup()
            {
                SubscriptionId = subscriptionId.Value,
                Region = orchestratorContext.TeamCloud.Configuration.Azure.Region,
                ResourceGroupId = (string)deploymentOutput.GetValueOrDefault("resourceGroupId", default(string)),
                ResourceGroupName = (string)deploymentOutput.GetValueOrDefault("resourceGroupName", default(string))
            };
        }
    }
}