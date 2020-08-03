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
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal;
using TeamCloud.Model.Internal.Commands;
using TeamCloud.Model.Internal.Data;
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
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorProjectCreateCommand>();
            var commandResult = command.CreateResult();

            using (log.BeginCommandScope(command))
            {
                var project = commandResult.Result = command.Payload;

                try
                {
                    try
                    {
                        commandResult = await ProvisionAsync(functionContext, command, log)
                            .ConfigureAwait(true);
                    }
                    catch
                    {
                        await RollbackAsync(functionContext, command, log)
                            .ConfigureAwait(true);

                        throw;
                    }
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);

                    throw;
                }
                finally
                {
                    var commandException = commandResult.Errors?.ToException();

                    if (commandException is null)
                        functionContext.SetCustomStatus($"Command succeeded", log);
                    else
                        functionContext.SetCustomStatus($"Command failed: {commandException.Message}", log, commandException);

                    functionContext.SetOutput(commandResult);
                }
            }
        }

        private static async Task<OrchestratorProjectCreateCommandResult> ProvisionAsync(IDurableOrchestrationContext functionContext, OrchestratorProjectCreateCommand command, ILogger log)
        {
            var teamCloud = await functionContext
                .GetTeamCloudAsync()
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();

            var project = commandResult.Result = command.Payload;
            project.Tags = teamCloud.Tags.Override(project.Tags);

            var projectUsers = project.Users.ToList();

            var providers = await functionContext
                .ListProvidersAsync(project.Type.Providers.Select(p => p.Id).ToList())
                .ConfigureAwait(true);

            var providerUserTasks = providers
                .Where(p => p.PrincipalId.HasValue)
                .Select(p => functionContext.GetUserAsync(p.PrincipalId.Value.ToString(), allowUnsafe: true));

            var providerUsers = await Task.WhenAll(providerUserTasks)
                .ConfigureAwait(true);

            foreach (var u in providerUsers)
                u.EnsureProjectMembership(project.Id, ProjectUserRole.Provider);

            projectUsers.AddRange(providerUsers);

            using (await functionContext.LockContainerDocumentAsync(project).ConfigureAwait(true))
            {
                functionContext.SetCustomStatus($"Creating project", log);

                project = commandResult.Result = await functionContext
                    .CreateProjectAsync(project)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Adding users", log);

                project.Users = await Task
                    .WhenAll(projectUsers.Select(user => functionContext.SetUserProjectMembershipAsync(user, project.Id, allowUnsafe: true)))
                    .ConfigureAwait(true);
            }

            functionContext.SetCustomStatus($"Allocating subscription", log);

            var subscriptionId = await functionContext
                .CallActivityWithRetryAsync<Guid>(nameof(ProjectSubscriptionSelectActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Initializing subscription", log);

            await functionContext
                .InitializeSubscriptionAsync(subscriptionId, waitFor: false)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Provisioning resources", log);

            var deploymentOutput = await functionContext
                .CallDeploymentAsync(nameof(ProjectResourcesCreateActivity), (project, subscriptionId))
                .ConfigureAwait(true);

            using (await functionContext.LockContainerDocumentAsync(project).ConfigureAwait(true))
            {
                functionContext.SetCustomStatus($"Provisioning identity", log);

                project.Identity = await functionContext
                    .CallActivityWithRetryAsync<ProjectIdentity>(nameof(ProjectIdentityCreateActivity), project)
                    .ConfigureAwait(true);

                project.ResourceGroup = new AzureResourceGroup()
                {
                    SubscriptionId = subscriptionId,
                    Region = project.Type.Region,
                    Id = (string)deploymentOutput.GetValueOrDefault("resourceGroupId", default(string)),
                    Name = (string)deploymentOutput.GetValueOrDefault("resourceGroupName", default(string))
                };

                project = commandResult.Result = await functionContext
                    .SetProjectAsync(project)
                    .ConfigureAwait(true);
            }

            functionContext.SetCustomStatus($"Tagging resources", log);

            await functionContext
                .CallActivityWithRetryAsync(nameof(ProjectResourcesTagActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Registering required resource providers", log);

            await functionContext
                .RegisterResourceProvidersAsync(project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Sending provider commands", log);

            var providerCommand = new ProviderProjectCreateCommand
            (
                command.BaseApi,
                command.User.PopulateExternalModel(),
                project.PopulateExternalModel(),
                command.CommandId
            );

            var providerResults = await functionContext
                .SendProviderCommandAsync<ProviderProjectCreateCommand, ProviderProjectCreateCommandResult>(providerCommand, project, failFast: true)
                .ConfigureAwait(true);

            var providerException = providerResults.Values?
                .SelectMany(result => result.Errors ?? new List<CommandError>())
                .ToException();

            if (providerException != null)
                throw providerException;

            return commandResult;
        }

        private static async Task RollbackAsync(IDurableOrchestrationContext functionContext, OrchestratorProjectCreateCommand command, ILogger log)
        {
            functionContext.SetCustomStatus($"Refreshing project", log);

            var project = (await functionContext
                .GetProjectAsync(command.ProjectId, allowUnsafe: true)
                .ConfigureAwait(true)) ?? command.Payload;

            functionContext.SetCustomStatus($"Rolling back project", log);

            var systemUser = await functionContext
                .CallActivityWithRetryAsync<UserDocument>(nameof(TeamCloudSystemUserActivity), null)
                .ConfigureAwait(true);

            var deleteCommand = new OrchestratorProjectDeleteCommand(command.BaseApi, systemUser, project);

            await functionContext
                .CallSubOrchestratorWithRetryAsync(nameof(OrchestratorProjectDeleteCommandOrchestration), deleteCommand)
                .ConfigureAwait(true);
        }
    }
}
