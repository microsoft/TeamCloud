/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Context;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations
{
    public static class ProjectCreateOrchestration
    {
        [FunctionName(nameof(ProjectCreateOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            (OrchestratorContext orchestratorContext, ProjectCreateCommand command) = functionContext.GetInput<(OrchestratorContext, ProjectCreateCommand)>();

            var user = command.User;
            var project = command.Payload;
            var teamCloud = orchestratorContext.TeamCloud;

            functionContext.SetCustomStatus("Creating Project ...");

            project.TeamCloudId = teamCloud.Id;
            project.TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey;
            project.ProviderVariables = teamCloud.Configuration.Providers.Select(p => (p.Id, p.Variables)).ToDictionary(t => t.Id, t => t.Variables);

            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectCreateActivity), project)
                .ConfigureAwait(true);

            await CreateAzureResourceGroupAsync(functionContext, orchestratorContext, project, teamCloud).ConfigureAwait(false);

            var projectContext = new ProjectContext(teamCloud, project, command.User);

            // Create and initialize providers...
            await CreateProjectResourcesAsync(functionContext, teamCloud, projectContext).ConfigureAwait(false);
            await InitializeProjectResourcesAsync(functionContext, teamCloud, projectContext).ConfigureAwait(false);

            functionContext.SetOutput(project);
        }

        private static async Task CreateAzureResourceGroupAsync(IDurableOrchestrationContext functionContext, OrchestratorContext orchestratorContext, Project project, TeamCloudInstance teamCloud)
        {
            var subscriptionId = await functionContext
                .CallActivityAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), teamCloud)
                .ConfigureAwait(true);

            project.ResourceGroup = await functionContext
                .CallActivityAsync<AzureResourceGroup>(nameof(AzureResourceGroupCreateActivity), (orchestratorContext, project, subscriptionId))
                .ConfigureAwait(true);

            // Update the project with the new resource group object
            project = await functionContext
                .CallActivityAsync<Project>(nameof(ProjectUpdateActivity), project)
                .ConfigureAwait(true);
        }

        private static Task InitializeProjectResourcesAsync(IDurableOrchestrationContext functionContext, TeamCloudInstance teamCloud, ProjectContext projectContext)
        {
            functionContext.SetCustomStatus("Initializing Project Resources...");

            // Create dictionary for providers and their dependencies
            var initProviders = new Dictionary<TeamCloudProviderConfiguration, List<string>>();
            foreach (var provider in teamCloud.Configuration.Providers)
            {
                // Ensure dependency IDs exist in the providers list
                var dependencyIds = provider.Dependencies.Init.Intersect(teamCloud.Configuration.Providers.Select(s => s.Id));

                initProviders.Add(provider, dependencyIds.ToList());
            }

            // Organize providers into a list of lists where each list contains a list of providers that should be created together
            var initProviderGroupings = PrioritizedProviderGroupings(initProviders);

            foreach (var group in initProviderGroupings)
            {
                // TODO: call init on all providers (handeling dependencies)
                // var tasks = teamCloud.Configuration.Providers.Select(p =>
                //                 functionContext.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

                // await Task.WhenAll(tasks);
            }

            return Task.CompletedTask;
        }

        private static Task CreateProjectResourcesAsync(IDurableOrchestrationContext functionContext, TeamCloudInstance teamCloud, ProjectContext projectContext)
        {
            functionContext.SetCustomStatus("Creating Project Resources...");

            // Create dictionary for providers and their dependencies
            var createProviders = new Dictionary<TeamCloudProviderConfiguration, List<string>>();
            foreach (var provider in teamCloud.Configuration.Providers)
            {
                // Ensure dependency IDs exist in the providers list
                var dependencyIds = provider.Dependencies.Create.Intersect(teamCloud.Configuration.Providers.Select(s => s.Id));

                createProviders.Add(provider, dependencyIds.ToList());
            }

            // Organize providers into a list of lists where each list contains a list of providers that should be created together
            var createProviderGroupings = PrioritizedProviderGroupings(createProviders);

            foreach (var group in createProviderGroupings)
            {
                // TODO: call create on all providers (handeling dependencies)
                //var tasks = teamCloud.Configuration.Providers.Select(p =>
                //                 functionContext.CallHttpAsync(HttpMethod.Post, p.Location, JsonConvert.SerializeObject(projectContext)));

                //await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Create a list of list of providers that are grouped together based on the order of dependencies. Each list contains a list of providers that should be created together. List is
        /// ordered based on how they should be priortized.
        /// </summary>
        /// <param name="allProviders"></param>
        /// <returns></returns>
        private static List<List<TeamCloudProviderConfiguration>> PrioritizedProviderGroupings(Dictionary<TeamCloudProviderConfiguration, List<string>> allProviders)
        {
            // Dictionary to store which phase the provider should be created in
            var prioritizedProviders = new Dictionary<TeamCloudProviderConfiguration, int>();

            // Recursive function to order the providers into groups based on when they should be executed
            OrganizeProviders(0, allProviders, prioritizedProviders);

            // Represents a list of list of providers based on phased order of when they should be executed
            var list = new List<List<TeamCloudProviderConfiguration>>();

            // Group the providers by phase in which they should be executed and loop through
            foreach (var priorityLevel in prioritizedProviders.OrderBy(o => o.Value).GroupBy(g => g.Value))
            {
                list.Add(priorityLevel.Select(s => s.Key).ToList());
            }

            return list;

            // Private recursive method to organize providers
            void OrganizeProviders(int phase, Dictionary<TeamCloudProviderConfiguration, List<string>> remainingProviders, Dictionary<TeamCloudProviderConfiguration, int> prioritizedList)
            {
                // Populate providers that don't have any more dependencies
                var noDependencyProvider = remainingProviders.Where(s => s.Value.Count == 0);
                foreach (var provider in noDependencyProvider)
                {
                    prioritizedList.Add(provider.Key, phase);
                }

                // Create list of remaining providers which have dependencies
                var dependentProviders = remainingProviders.Where(s => s.Value.Count > 0);

                // Override the list providers and their dependencies which be used to tracking remaining dependent providers in the recursive call
                remainingProviders = new Dictionary<TeamCloudProviderConfiguration, List<string>>();

                foreach (var dependentProvider in dependentProviders)
                {
                    // Remove any dependencies in provider dependencies that were already marked in the prioritized list
                    var remainingDependencyIDs = dependentProvider.Value.Where(s =>
                        prioritizedList.Any(a => a.Key.Id.Equals(s, StringComparison.InvariantCultureIgnoreCase)) // Ensure that the dependent provider isn't already added of prioritize providers list
                        && !dependentProvider.Key.Id.Contains(s, StringComparison.InvariantCultureIgnoreCase) // Ensure that this provider isn't relying on itself
                        ).ToList();

                    // Add provider to list of providers that need to be generated
                    remainingProviders.Add(dependentProvider.Key, remainingDependencyIDs);
                }

                // If there are still dependent providers, recurse with updated phase number
                if (remainingProviders.Count > 0)
                    OrganizeProviders(++phase, remainingProviders, prioritizedList);
            }
        }
    }
}