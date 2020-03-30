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
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Commands;
using TeamCloud.Orchestrator.Orchestrations.Commands.Activities;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class CommandSendOrchestration
    {
        [FunctionName(nameof(CommandSendOrchestration))]
        public static async Task<ICommandResult> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (command, provider) = functionContext.GetInput<(IProviderCommand, Provider)>();

            var commandResult = command.CreateResult();

            // if the provider to use only supports the simple command mode
            // we can not send commands like add, update, or delete project users. 
            // instead we need to send a full project update to process the command!

            if (provider.CommandMode == ProviderCommandMode.Simple && IsExtendedProjectCommand())
            {
                commandResult = await SwitchCommandAsync(functionContext, provider, command, commandResult, log)
                    .ConfigureAwait(true);
            }
            else
            {
                commandResult = await ProcessCommandAsync(functionContext, provider, command, commandResult, log)
                    .ConfigureAwait(true);
            }

            return commandResult;

            // commands indentified as extended project commands can't processed by
            // a provider that support the simple provider command mode.

            bool IsExtendedProjectCommand()
                => command.ProjectId.HasValue && !(command is IProviderCommand<Project>);
        }

        private static Task RegisterProviderAsync(IDurableOrchestrationContext functionContext, IProviderCommand providerCommand, Provider provider)
        {
            if (providerCommand is ProviderRegisterCommand || provider.Registered.HasValue)
                return Task.CompletedTask;

            return functionContext
                .RegisterProviderAsync(provider, true);
        }

        private static Task EnableProviderAsync(IDurableOrchestrationContext functionContext, IProviderCommand providerCommand, Provider provider)
        {
            if (!providerCommand.ProjectId.HasValue || !provider.PrincipalId.HasValue)
                return Task.CompletedTask;

            return functionContext
                .CallActivityWithRetryAsync(nameof(ProjectResourcesAccessActivity), (providerCommand.ProjectId.Value, provider.PrincipalId.Value));
        }


        private static async Task<IProviderCommand> AugmentCommandAsync(IDurableOrchestrationContext functionContext, Provider provider, IProviderCommand command)
        {
            var teamCloud = await functionContext
                .GetTeamCloudAsync()
                .ConfigureAwait(true);

            var providerProperties = teamCloud.Properties;

            if (command.ProjectId.HasValue)
            {
                var project = await functionContext
                    .GetProjectAsync(command.ProjectId.Value)
                    .ConfigureAwait(true);

                var providerReference = project.Type.Providers
                    .Single(pr => pr.Id == provider.Id);

                command.Properties = providerProperties.Resolve(project)
                    .Override(project.Type.Properties.Resolve(project))
                    .Override(project.Properties.Resolve(project))
                    .Override(provider.Properties.Resolve(project))
                    .Override(providerReference.Properties.Resolve(project));
            }
            else
            {
                command.Properties = providerProperties.Resolve()
                    .Override(provider.Properties.Resolve());
            }

            if (command.Payload is IProperties payloadWithProperties)
            {
                payloadWithProperties.Properties = payloadWithProperties.Properties.Resolve(command.Payload);
            }

            return command;
        }

        private static async Task<ICommandResult> ProcessCommandAsync(IDurableOrchestrationContext functionContext, Provider provider, IProviderCommand command, ICommandResult commandResult, ILogger log)
        {
            var commandMessage = default(ICommandMessage);
            var commandCallback = default(string);

            try
            {
                functionContext.SetCustomStatus($"Acquire callback url for command '{command.CommandId}'", log);

                commandCallback = await functionContext
                    .CallActivityWithRetryAsync<string>(nameof(CallbackAcquireActivity), (functionContext.InstanceId, command))
                    .ConfigureAwait(true);

                commandMessage = new ProviderCommandMessage(command, commandCallback);

                functionContext.SetCustomStatus($"Prepare sending command '{command.CommandId}'", log);

                await RegisterProviderAsync(functionContext, command, provider)
                    .ConfigureAwait(true);

                await EnableProviderAsync(functionContext, command, provider)
                    .ConfigureAwait(true);

                command = await AugmentCommandAsync(functionContext, provider, command)
                    .ConfigureAwait(true);

                await functionContext
                    .AuditAsync(provider, command, commandResult)
                    .ConfigureAwait(true);

                try
                {
                    functionContext.SetCustomStatus($"Sending command '{command.CommandId}'", log);

                    commandResult = await functionContext
                        .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandSendActivity), (provider, commandMessage))
                        .ConfigureAwait(true);
                }
                catch (RetryCanceledException)
                {
                    commandResult = await functionContext
                        .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultActivity), (provider, commandMessage))
                        .ConfigureAwait(true);
                }
                finally
                {
                    await functionContext
                        .AuditAsync(provider, command, commandResult)
                        .ConfigureAwait(true);
                }

                if (commandResult.RuntimeStatus.IsActive())
                {
                    var commandTimeout = (commandResult.Timeout > TimeSpan.Zero && commandResult.Timeout < CommandResult.MaximumTimeout)
                        ? commandResult.Timeout         // use the timeout reported back by the provider
                        : CommandResult.MaximumTimeout; // use the defined maximum timeout 

                    functionContext.SetCustomStatus($"Waiting for command ({command.CommandId}) result on {commandCallback} for {commandTimeout}", log);

                    commandResult = await functionContext
                        .WaitForExternalEvent<ICommandResult>(command.CommandId.ToString(), commandTimeout, null)
                        .ConfigureAwait(true);

                    if (commandResult is null)
                    {
                        commandResult = await functionContext
                            .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultActivity), (provider, commandMessage))
                            .ConfigureAwait(true);

                        throw new TimeoutException($"Provider '{provider.Id}' ran into timeout ({commandTimeout})");
                    }
                }
            }
            catch (Exception exc)
            {
                functionContext.SetCustomStatus($"Sending command '{command.CommandId}' failed: {exc.Message}", log, exc);

                // ensure we always have a command result
                // to add our exception so we won't break
                // our command auditing in the finally block

                commandResult ??= command.CreateResult();
                commandResult.Errors.Add(exc);
            }
            finally
            {
                if (!string.IsNullOrEmpty(commandCallback))
                {
                    functionContext.SetCustomStatus($"Invalidating callback url for command '{command.CommandId}'", log);

                    await functionContext
                        .CallActivityWithRetryAsync(nameof(CallbackInvalidateActivity), functionContext.InstanceId)
                        .ConfigureAwait(true);
                }

                await functionContext
                    .AuditAsync(provider, command, commandResult)
                    .ConfigureAwait(true);

                await ProcessOutputAsync(functionContext, provider, command, commandResult)
                    .ConfigureAwait(true);
            }

            return commandResult;
        }

        private static async Task<ICommandResult> SwitchCommandAsync(IDurableOrchestrationContext functionContext, Provider provider, IProviderCommand command, ICommandResult commandResult, ILogger log)
        {
            try
            {
                functionContext.SetCustomStatus($"Switching mode for command '{command.CommandId}'", log);

                var project = await functionContext
                    .GetProjectAsync(command.ProjectId.Value)
                    .ConfigureAwait(true);

                functionContext.ContinueAsNew((
                    new ProviderProjectUpdateCommand(command.User, project, command.CommandId),
                    provider
                ));
            }
            catch (Exception exc)
            {
                functionContext.SetCustomStatus($"Switching mode for command '{command.CommandId}' failed: {exc.Message}", log, exc);

                commandResult ??= command.CreateResult();
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        private static async Task ProcessOutputAsync(IDurableOrchestrationContext functionContext, Provider provider, IProviderCommand command, ICommandResult commandResult)
        {
            if (command.ProjectId.HasValue && commandResult is ICommandResult<ProviderOutput> providerOutputResult)
            {
                using (await functionContext.LockAsync<Project>(command.ProjectId.Value.ToString()).ConfigureAwait(true))
                {
                    var project = await functionContext
                        .GetProjectAsync(command.ProjectId.Value)
                        .ConfigureAwait(true);

                    var providerReference = project.Type.Providers
                        .SingleOrDefault(pr => pr.Id == provider.Id);

                    if (providerReference != null)
                    {
                        var commandType = command.GetType().Name;

                        var resultProperties = providerOutputResult?.Result?.Properties ?? new Dictionary<string, string>();

                        if (!providerReference.Metadata.TryAdd(commandType, resultProperties))
                        {
                            providerReference.Metadata[commandType] =
                                (providerReference.Metadata[commandType] ?? new Dictionary<string, string>()).Override(resultProperties);
                        }

                        project = await functionContext
                            .SetProjectAsync(project)
                            .ConfigureAwait(true);
                    }
                }
            }
        }
    }
}
