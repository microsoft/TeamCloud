/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class AzureResourceGroupCreateActivity
    {
        private readonly IAzureSessionFactory azureSessionFactory;

        public AzureResourceGroupCreateActivity(IAzureSessionFactory azureSessionFactory)
        {
            this.azureSessionFactory = azureSessionFactory ?? throw new ArgumentNullException(nameof(azureSessionFactory));
        }

        [FunctionName(nameof(AzureResourceGroupCreateActivity))]
        public async Task<Project> RunActivity(
            [ActivityTrigger] (TeamCloudInstance teamCloud, Project project, Guid subscriptionID) input)
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            if (input.teamCloud == null)
                throw new ArgumentNullException(nameof(input.teamCloud));
            if (input.project == null)
                throw new ArgumentNullException(nameof(input.project));
            if (input.subscriptionID == null)
                throw new ArgumentNullException(nameof(input.subscriptionID));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            // Determine the group name
            var resourceGroupName = $"{input.teamCloud.Configuration.Azure.ResourceGroupNamePrefix}{input.project.Name}";
            this.ValidateAzureGroupName(resourceGroupName);

            // Initialize the resource group with appropriate settings
            var rg = input.project.ResourceGroup = new AzureResourceGroup
            {
                SubscriptionId = input.subscriptionID.ToString(),
                ResourceGroupName = resourceGroupName,
                Region = input.teamCloud.Configuration.Azure.Region
            };

            // Create instance to Azure instance
            var azureSession = azureSessionFactory.CreateSession(input.subscriptionID);

            // Determine a unique group name and append post fix
            int postfixCounter = 0;
            while (await azureSession.ResourceGroups.ContainAsync(rg.ResourceGroupName).ConfigureAwait(false))
            {
                postfixCounter++;

                // Append counter to resource group name
                rg.ResourceGroupName = resourceGroupName + postfixCounter;
                this.ValidateAzureGroupName(rg.ResourceGroupName);

                // Abort this groupname if there are just too many projects with this group name
                if (postfixCounter > 100)
                    throw new ArgumentOutOfRangeException($"Too many resource groups with the name '{resourceGroupName}', use another name.");
            }

            // Create new group with unique resource group name
            var newGroup = await azureSession.ResourceGroups
                .Define(rg.ResourceGroupName)
                .WithRegion(rg.Region)
                .CreateAsync()
                .ConfigureAwait(false);

            // Assign new generated ID back to AzureResourceGroup ID property of the project
            input.project.ResourceGroup.Id = Guid.Parse(newGroup.Id);

            // Return project back to caller
            return input.project;
        }

        private void ValidateAzureGroupName(string name)
        {
            // Resource group name length from: https://docs.microsoft.com/en-us/azure/architecture/best-practices/resource-naming
            if (string.IsNullOrEmpty(name) || name.Length < 1 || name.Length > 90)
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentOutOfRangeException($"Azure resource group name '{name}' should be 1-90 characters.");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            // Ensure that the resource group name is valid per regex from: https://docs.microsoft.com/en-us/rest/api/resources/resourcegroups/createorupdate
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[-\w\._\(\)]+$");
            if (!regex.IsMatch(name))
                throw new ArgumentException($"Invalid resource group name of '{name}'");
        }
    }
}