/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
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
                functionContext.SetCustomStatus($"Command {command.CommandId} ({command.GetType()}) - processing ...", log);

                await functionContext
                    .WaitForProjectCommandsAsync(command)
                    .ConfigureAwait(true);

                try
                {
                    functionContext.SetCustomStatus($"Command {command.CommandId} ({command.GetType()}) - provisioning ...", log);

                    await ProvisionAsync(functionContext, command, commandResult, log)
                        .ConfigureAwait(true);
                }
                catch (Exception provisioningExc)
                {
                    commandResult.Errors.Add(provisioningExc);

                    try
                    {
                        functionContext.SetCustomStatus($"Command {command.CommandId} ({command.GetType()}) - rolling back ...", log);

                        await RollbackAsync(functionContext, command, commandResult, log)
                            .ConfigureAwait(true);
                    }
                    catch (Exception rollbackExc)
                    {
                        commandResult.Errors.Add(rollbackExc);
                    }

                    throw;
                }
            }
            catch (Exception processingExc)
            {
                if (!commandResult.Errors.Contains(processingExc))
                    commandResult.Errors.Add(processingExc);

                throw;
            }
            finally
            {
                if (commandResult.Errors.Any())
                {
                    var commandExc = commandResult.Errors.Count == 1
                        ? commandResult.Errors.First()
                        : new AggregateException(commandResult.Errors);

                    functionContext.SetCustomStatus($"Command {command.CommandId} ({command.GetType()}) - failed with {commandResult.Errors.Count} error/s", log, commandExc);
                }
                else
                {
                    functionContext.SetCustomStatus($"Command {command.CommandId} ({command.GetType()}) - succeeded", log);
                }

                functionContext.SetOutput(commandResult);
            }
        }

        private static async Task ProvisionAsync(IDurableOrchestrationContext functionContext, OrchestratorProjectCreateCommand command, OrchestratorProjectCreateCommandResult commandResult, ILogger log)
        {
            var teamCloud = await functionContext
                .GetTeamCloudAsync()
                .ConfigureAwait(true);

            var project = command.Payload;
            project.TeamCloudId = teamCloud.Id;
            project.Tags = teamCloud.Tags.Merge(project.Tags);
            project.Properties = teamCloud.Properties.Merge(project.Properties);

            functionContext.SetCustomStatus($"Project {project.Id} - Creating ...", log);

            project = await functionContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectCreateActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Project {project.Id} - Allocating subscription ...", log);

            var subscriptionId = await functionContext
                .CallActivityWithRetryAsync<Guid>(nameof(AzureSubscriptionPoolSelectActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Project {project.Id} - Provisioning resource group ...", log);

            project.ResourceGroup = await functionContext
                .CallActivityWithRetryAsync<AzureResourceGroup>(nameof(AzureResourceGroupCreateActivity), (project, subscriptionId))
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Project {project.Id} - Updating resource group tags ...", log);

            await functionContext
                .CallActivityWithRetryAsync(nameof(AzureResourceGroupTagActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Project {project.Id} - Updating ...", log);

            project = await functionContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectUpdateActivity), project)
                .ConfigureAwait(true);

            functionContext.SetCustomStatus($"Project {project.Id} - Preparing provider command ...", log);

            var providerCommand = command is IOrchestratorCommandConvert<Project> commandConvert
                ? commandConvert.CreateProviderCommand(project)
                : throw new NotSupportedException($"Unable to convert command of type '{command.GetType()}' to '{typeof(IProviderCommand)}'");

            functionContext.SetCustomStatus($"Project {project.Id} - Sending provider command ...", log);

            var providerResults = await functionContext
                .SendCommandAsync<ICommandResult<ProviderOutput>>(providerCommand, project)
                .ConfigureAwait(true);

            project.Merge(providerResults);

            project = await functionContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectUpdateActivity), project)
                .ConfigureAwait(true);

            commandResult.Result = project;
        }

        private static async Task RollbackAsync(IDurableOrchestrationContext functionContext, OrchestratorProjectCreateCommand command, OrchestratorProjectCreateCommandResult commandResult, ILogger log)
        {
            var systemUser = await functionContext
                .CallActivityWithRetryAsync<User>(nameof(TeamCloudUserActivity), null)
                .ConfigureAwait(true);

            var deleteCommand = new OrchestratorProjectDeleteCommand(systemUser, command.Payload);

            functionContext.StartNewOrchestration(nameof(OrchestratorProjectDeleteCommandOrchestration), new OrchestratorCommandMessage(deleteCommand));
        }
    }
}
