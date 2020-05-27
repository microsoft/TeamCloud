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
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
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

            functionContext.SetCustomStatus($"Creating project", log);

            project = commandResult.Result = await functionContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectCreateActivity), project)
                .ConfigureAwait(true);

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
                functionContext.SetCustomStatus($"Updating project", log);

                project.ResourceGroup = new AzureResourceGroup()
                {
                    SubscriptionId = subscriptionId,
                    Region = project.Type.Region,
                    Id = (string)deploymentOutput.GetValueOrDefault("resourceGroupId", default(string)),
                    Name = (string)deploymentOutput.GetValueOrDefault("resourceGroupName", default(string))
                };

                project.KeyVault = new AzureKeyVault()
                {
                    VaultId = (string)deploymentOutput.GetValueOrDefault("vaultId", default(string)),
                    VaultName = (string)deploymentOutput.GetValueOrDefault("vaultName", default(string)),
                    VaultUrl = (string)deploymentOutput.GetValueOrDefault("vaultUrl", default(string))
                };

                project = commandResult.Result = await functionContext
                    .SetProjectAsync(project)
                    .ConfigureAwait(true);
            }

            functionContext.SetCustomStatus($"Tagging resources", log);

            await functionContext
                .CallActivityWithRetryAsync(nameof(ProjectResourcesTagActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Creating project identity", log);

            await functionContext
                .CallActivityWithRetryAsync(nameof(ProjectIdentityCreateActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Sending provider commands", log);

            var providerResults = await functionContext
                .SendCommandAsync<ProviderProjectCreateCommand, ProviderProjectCreateCommandResult>(new ProviderProjectCreateCommand(command.User, project, command.CommandId), failFast: true)
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
                .GetProjectAsync(command.ProjectId.GetValueOrDefault(), allowUnsafe: true)
                .ConfigureAwait(true)) ?? command.Payload;

            functionContext.SetCustomStatus($"Rolling back project", log);

            var systemUser = await functionContext
                .CallActivityWithRetryAsync<User>(nameof(TeamCloudSystemUserActivity), null)
                .ConfigureAwait(true);

            var deleteCommand = new OrchestratorProjectDeleteCommand(systemUser, project);

            await functionContext
                .CallSubOrchestratorWithRetryAsync(nameof(OrchestratorProjectDeleteCommandOrchestration), deleteCommand)
                .ConfigureAwait(true);
        }
    }
}
