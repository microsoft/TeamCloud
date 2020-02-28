/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Deployment;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Templates;

namespace TeamCloud.Orchestrator.Orchestrations.Azure
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
            [ActivityTrigger] (Project project, Guid subscriptionId) input,
            ILogger log)
        {
            var project = input.project;
            var subscriptionId = input.subscriptionId;

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
            template.Parameters["projectPrefix"] = project.Type.ResourceGroupNamePrefix;
            template.Parameters["resourceGroupName"] = project.ResourceGroup?.ResourceGroupName; // if null - the template generates a unique name
            template.Parameters["resourceGroupLocation"] = project.ResourceGroup?.Region ?? project.Type.Region;

            try
            {
                var deployment = await azureDeploymentService
                    .DeployTemplateAsync(template, subscriptionId)
                    .ConfigureAwait(false);

                var deploymentOutput = await deployment
                    .WaitAndGetOutputAsync(throwOnError: true, cleanUp: true)
                    .ConfigureAwait(false);

                return new AzureResourceGroup()
                {
                    SubscriptionId = subscriptionId,
                    Region = project.Type.Region,
                    ResourceGroupId = (string)deploymentOutput.GetValueOrDefault("resourceGroupId", default(string)),
                    ResourceGroupName = (string)deploymentOutput.GetValueOrDefault("resourceGroupName", default(string))
                };
            }
            catch (AzureDeploymentException deployEx)
            {
                log.LogError(deployEx, $"Error deploying new Resource Group for Project.\n {deployEx.ResourceError}");
                throw;
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Error deploying new Resource Group for Project.");
                throw;
            }
        }
    }
}
