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
            [ActivityTrigger] (OrchestratorContext, Project, Guid) input)
        {
            var orchestratorContext = input.Item1;
            var project = input.Item2;
            var subscriptionId = input.Item3;

            if (orchestratorContext == null)
                throw new ArgumentNullException(nameof(input));

            if (project == null)
                throw new ArgumentNullException(nameof(input));


            // if the provided project instance is already assigned
            // to a subscription we use this one instead of the provided
            // one to make our activity idempotent (we always go to the
            // same subscription). the same is valid for the projects
            // resource group name and location (passed as templated params).

            subscriptionId = project.ResourceGroup?.SubscriptionId ?? subscriptionId;

            var template = new CreateProjectTemplate();

            template.Parameters["projectId"] = project.Id;
            template.Parameters["projectName"] = project.Name;
            template.Parameters["projectPrefix"] = orchestratorContext.TeamCloud.Configuration.Azure.ResourceGroupNamePrefix;
            template.Parameters["resourceGroupName"] = project.ResourceGroup?.ResourceGroupName; // if null - the template generates a unique name
            template.Parameters["resourceGroupLocation"] = project.ResourceGroup?.Region ?? orchestratorContext.TeamCloud.Configuration.Azure.Region;

            var deployment = await azureDeploymentService
                .DeployTemplateAsync(template, subscriptionId)
                .ConfigureAwait(false);

            _ = await deployment
                .WaitAsync(throwOnError: true)
                .ConfigureAwait(false);

            var deploymentOutput = await deployment
                .GetOutputAsync()
                .ConfigureAwait(false);

            return new AzureResourceGroup()
            {
                SubscriptionId = subscriptionId,
                Region = orchestratorContext.TeamCloud.Configuration.Azure.Region,
                ResourceGroupId = (string)deploymentOutput.GetValueOrDefault("resourceGroupId", default(string)),
                ResourceGroupName = (string)deploymentOutput.GetValueOrDefault("resourceGroupName", default(string))
            };
        }
    }
}