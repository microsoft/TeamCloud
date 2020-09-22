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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Internal;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProjectCreateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectCreateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestrationContext.GetInput<OrchestratorProjectCreateCommand>();
            var commandResult = command.CreateResult();

            using (log.BeginCommandScope(command))
            {
                var project = commandResult.Result = command.Payload;

                try
                {
                    try
                    {
                        commandResult = await ProvisionAsync(orchestrationContext, command, log)
                            .ConfigureAwait(true);
                    }
                    catch
                    {
                        await RollbackAsync(orchestrationContext, command, log)
                            .ConfigureAwait(true);

                        throw;
                    }
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);
                }
                finally
                {
                    var commandException = commandResult.Errors?.ToException();

                    if (commandException is null)
                        orchestrationContext.SetCustomStatus($"Command succeeded", log);
                    else
                        orchestrationContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                    orchestrationContext.SetOutput(commandResult);
                }
            }
        }

        private static async Task<OrchestratorProjectCreateCommandResult> ProvisionAsync(IDurableOrchestrationContext orchestrationContext, OrchestratorProjectCreateCommand command, ILogger log)
        {
            var teamCloud = await orchestrationContext
                .GetTeamCloudAsync()
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();

            var project = commandResult.Result = command.Payload;
            project.Tags = teamCloud.Tags.Override(project.Tags);

            var projectUsers = project.Users.ToList();

            var providers = await orchestrationContext
                .ListProvidersAsync(project.Type.Providers.Select(p => p.Id).ToList())
                .ConfigureAwait(true);

            var providerUserTasks = providers
                .Where(p => p.PrincipalId.HasValue)
                .Select(p => orchestrationContext.GetUserAsync(p.PrincipalId.Value.ToString(), allowUnsafe: true));

            var providerUsers = await Task.WhenAll(providerUserTasks)
                .ConfigureAwait(true);

            foreach (var u in providerUsers)
                u.EnsureProjectMembership(project.Id, ProjectUserRole.Provider);

            projectUsers.AddRange(providerUsers);

            using (await orchestrationContext.LockContainerDocumentAsync(project).ConfigureAwait(true))
            {
                orchestrationContext.SetCustomStatus($"Creating project", log);

                project = commandResult.Result = await orchestrationContext
                    .CreateProjectAsync(project)
                    .ConfigureAwait(true);

                orchestrationContext.SetCustomStatus($"Adding users", log);

                project.Users = await Task
                    .WhenAll(projectUsers.Select(user => orchestrationContext.SetUserProjectMembershipAsync(user, project.Id, allowUnsafe: true)))
                    .ConfigureAwait(true);
            }

            orchestrationContext.SetCustomStatus($"Allocating subscription", log);

            var subscriptionId = await orchestrationContext
                .CallActivityWithRetryAsync<Guid>(nameof(ProjectSubscriptionSelectActivity), project)
                .ConfigureAwait(true);

            orchestrationContext.SetCustomStatus($"Initializing subscription", log);

            await orchestrationContext
                .InitializeSubscriptionAsync(subscriptionId, waitFor: false)
                .ConfigureAwait(true);

            orchestrationContext.SetCustomStatus($"Provisioning resources", log);

            var deploymentOutput = await orchestrationContext
                .CallDeploymentAsync(nameof(ProjectResourcesCreateActivity), new ProjectResourcesCreateActivity.Input() { Project = project, SubscriptionId = subscriptionId })
                .ConfigureAwait(true);

            using (await orchestrationContext.LockContainerDocumentAsync(project).ConfigureAwait(true))
            {
                project.ResourceGroup = new AzureResourceGroup()
                {
                    SubscriptionId = subscriptionId,
                    Region = project.Type.Region,
                    Id = (string)deploymentOutput.GetValueOrDefault("resourceGroupId", default(string)),
                    Name = (string)deploymentOutput.GetValueOrDefault("resourceGroupName", default(string))
                };

                orchestrationContext.SetCustomStatus($"Provisioning identity", log);

                project.Identity = await orchestrationContext
                    .CallActivityWithRetryAsync<ProjectIdentity>(nameof(ProjectIdentityCreateActivity), project)
                    .ConfigureAwait(true);

                project = commandResult.Result = await orchestrationContext
                    .SetProjectAsync(project)
                    .ConfigureAwait(true);
            }

            orchestrationContext.SetCustomStatus($"Tagging resources", log);

            await orchestrationContext
                .CallActivityWithRetryAsync(nameof(ProjectResourcesTagActivity), project)
                .ConfigureAwait(true);

            orchestrationContext.SetCustomStatus($"Registering required resource providers", log);

            await orchestrationContext
                .RegisterResourceProvidersAsync(project)
                .ConfigureAwait(true);

            orchestrationContext.SetCustomStatus($"Sending provider commands", log);

            var providerCommand = new ProviderProjectCreateCommand
            (
                command.User.PopulateExternalModel(),
                project.PopulateExternalModel(),
                command.CommandId
            );

            var providerResults = await orchestrationContext
                .SendProviderCommandAsync<ProviderProjectCreateCommand, ProviderProjectCreateCommandResult>(providerCommand, project, failFast: true)
                .ConfigureAwait(true);

            var providerException = providerResults.Values?
                .SelectMany(result => result.Errors ?? new List<CommandError>())
                .ToException();

            if (providerException != null)
                throw providerException;

            return commandResult;
        }

        private static async Task RollbackAsync(IDurableOrchestrationContext orchestrationContext, OrchestratorProjectCreateCommand command, ILogger log)
        {
            orchestrationContext.SetCustomStatus($"Refreshing project", log);

            var project = (await orchestrationContext
                .GetProjectAsync(command.ProjectId, allowUnsafe: true)
                .ConfigureAwait(true)) ?? command.Payload;

            orchestrationContext.SetCustomStatus($"Rolling back project", log);

            var systemUser = await orchestrationContext
                .CallActivityWithRetryAsync<UserDocument>(nameof(TeamCloudSystemUserActivity), null)
                .ConfigureAwait(true);

            var deleteCommand = new OrchestratorProjectDeleteCommand(systemUser, project);

            await orchestrationContext
                .CallSubOrchestratorWithRetryAsync(nameof(OrchestratorProjectDeleteCommandOrchestration), deleteCommand)
                .ConfigureAwait(true);
        }
    }
}
