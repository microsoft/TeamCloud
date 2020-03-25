/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;
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

            var commandMessage = functionContext.GetInput<OrchestratorCommandMessage>();
            var command = (OrchestratorProjectCreateCommand)commandMessage.Command;
            var commandResult = command.CreateResult();

            try
            {
                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                try
                {
                    commandResult = await ProvisionAsync(functionContext, command, log)
                        .ConfigureAwait(true);
                }
                catch
                {
                    await RollbackAsync(functionContext, command)
                        .ConfigureAwait(true);

                    throw;
                }
            }
            catch (Exception processingExc)
            {
                commandResult.Errors.Add(processingExc);

                throw;
            }
            finally
            {
                var commandException = commandResult.GetException();

                if (commandException is null)
                    functionContext.SetCustomStatus($"Command succeeded", log);
                else
                    functionContext.SetCustomStatus($"Command failed", log, commandException);

                functionContext.SetOutput(commandResult);
            }
        }

        private static async Task<OrchestratorProjectCreateCommandResult> ProvisionAsync(IDurableOrchestrationContext functionContext, OrchestratorProjectCreateCommand command, ILogger log)
        {
            var teamCloud = await functionContext
                .GetTeamCloudAsync()
                .ConfigureAwait(true);

            var commandResult = command.CreateResult();
            var project = commandResult.Result = command.Payload;

            // initialize the new project with some data
            // from the teamcloud instance:
            // - TeamCloudId = ensure that the new project belongs to a team cloud instance
            // - Tags = ensure that every project starts with a set of tags defined by the team cloud instance
            // CAUTION: there is no need to populate any other data (e.g. properties) on the new project instance

            project.TeamCloudId = teamCloud.Id;
            project.Tags = teamCloud.Tags.Override(project.Tags);

            functionContext.SetCustomStatus($"Creating project", log);

            project = commandResult.Result = await functionContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectCreateActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Allocating subscription", log);

            var subscriptionId = await functionContext
                .CallActivityWithRetryAsync<Guid>(nameof(ProjectSubscriptionSelectActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Provisioning resources", log);

            var (resourceGroup, keyVault) = await functionContext
                .CallActivityWithRetryAsync<(AzureResourceGroup, AzureKeyVault)>(nameof(ProjectResourcesCreateActivity), (project, subscriptionId))
                .ConfigureAwait(true);

            using (await functionContext.LockAsync(project).ConfigureAwait(true))
            {
                functionContext.SetCustomStatus($"Updating project", log);

                project.ResourceGroup = resourceGroup;
                project.KeyVault = keyVault;

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
                .GetException();

            if (providerException is null)
                return commandResult;

            throw providerException;
        }

        private static async Task RollbackAsync(IDurableOrchestrationContext functionContext, OrchestratorProjectCreateCommand command)
        {
            var systemUser = await functionContext
                .CallActivityWithRetryAsync<User>(nameof(TeamCloudUserActivity), null)
                .ConfigureAwait(true);

            var deleteCommand = new OrchestratorProjectDeleteCommand(systemUser, command.Payload);
            var deleteCommandMessage = new OrchestratorCommandMessage(deleteCommand);

            functionContext
                .StartNewOrchestration(nameof(OrchestratorProjectDeleteCommandOrchestration), deleteCommandMessage);
        }
    }
}
