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
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Auditing;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Serialization;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class ProviderSendOrchestration
    {
        [FunctionName(nameof(ProviderSendOrchestration))]
        public static async Task<ICommandResult> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (command, provider) = functionContext.GetInput<(IProviderCommand, Provider)>();
            var commandResult = command.CreateResult();
            var commandLog = functionContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

            using (log.BeginCommandScope(command, provider))
            using (functionContext.TrackCommandMetrics(command))
            {
                // if the provider to use only supports the simple command mode
                // we can not send commands like add, update, or delete project users.
                // instead we need to send a full project update to process the command!

                var isNotProjectCommandWithProjectId = provider.CommandMode == ProviderCommandMode.Simple
                                                    && !string.IsNullOrEmpty(command.ProjectId)
                                                    && !(command is IProviderCommand<Model.Data.Project>);

                commandResult = isNotProjectCommandWithProjectId
                    ? await SwitchCommandAsync(functionContext, provider, command, commandResult, commandLog).ConfigureAwait(true)
                    : await ProcessCommandAsync(functionContext, provider, command, commandResult, commandLog).ConfigureAwait(true);
            }

            return commandResult;
        }

        private static async Task<IProviderCommand> AugmentCommandAsync(IDurableOrchestrationContext functionContext, Provider provider, IProviderCommand command)
        {
            if (!string.IsNullOrEmpty(command.ProjectId))
            {
                var project = await functionContext
                    .GetProjectAsync(command.ProjectId, allowUnsafe: true)
                    .ConfigureAwait(true);

                var providerReference = project.Type.Providers
                    .Single(pr => pr.Id == provider.Id);

                command.Properties = provider.Properties.Resolve(project)
                    .Override(project.Type.Properties.Resolve(project))
                    .Override(project.Properties.Resolve(project))
                    .Override(provider.Properties.Resolve(project))
                    .Override(providerReference.Properties.Resolve(project));
            }
            else
            {
                command.Properties = provider.Properties.Resolve();
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
                functionContext.SetCustomStatus($"Acquire callback url", log);

                commandCallback = await functionContext
                    .CallActivityWithRetryAsync<string>(nameof(CallbackUrlGetActivity), (functionContext.InstanceId, command))
                    .ConfigureAwait(true);

                commandMessage = new ProviderCommandMessage(command, commandCallback);

                if (!(command is ProviderRegisterCommand || provider.Registered.HasValue))
                {
                    log.LogInformation($"Register provider {provider.Id} for command {command.CommandId}");

                    await functionContext
                        .RegisterProviderAsync(provider, true)
                        .ConfigureAwait(true);
                }

                if (!string.IsNullOrEmpty(command.ProjectId) && provider.PrincipalId.HasValue)
                {
                    log.LogInformation($"Enable provider {provider.Id} for command {command.CommandId}");

                    await functionContext
                        .CallActivityWithRetryAsync(nameof(ProjectResourcesAccessActivity), (command.ProjectId, provider.PrincipalId.Value))
                        .ConfigureAwait(true);
                }

                functionContext.SetCustomStatus($"Augmenting command", log);

                command = await AugmentCommandAsync(functionContext, provider, command)
                    .ConfigureAwait(true);

                await functionContext
                    .AuditAsync(command, commandResult, provider)
                    .ConfigureAwait(true);

                try
                {
                    functionContext.SetCustomStatus($"Sending command", log);

                    commandResult = await functionContext
                        .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandSendActivity), (provider, commandMessage))
                        .ConfigureAwait(true);
                }
                catch (RetryCanceledException)
                {
                    commandResult = await functionContext
                        .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultFetchActivity), (provider, commandMessage))
                        .ConfigureAwait(true);
                }
                finally
                {
                    await functionContext
                        .AuditAsync(command, commandResult, provider)
                        .ConfigureAwait(true);
                }

                if (commandResult.RuntimeStatus.IsActive())
                {
                    var commandTimeout = (commandResult.Timeout > TimeSpan.Zero && commandResult.Timeout < CommandResult.MaximumTimeout)
                        ? commandResult.Timeout         // use the timeout reported back by the provider
                        : CommandResult.MaximumTimeout; // use the defined maximum timeout

                    functionContext.SetCustomStatus($"Waiting for command result", log);

                    commandResult = await functionContext
                        .WaitForExternalEvent<ICommandResult>(command.CommandId.ToString(), commandTimeout, null)
                        .ConfigureAwait(true);

                    if (commandResult is null)
                    {
                        // provider ran into a timeout
                        // lets give our provider a last
                        // chance to return a command result

                        commandResult = await functionContext
                            .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultFetchActivity), (provider, commandMessage))
                            .ConfigureAwait(true);

                        if (commandResult.RuntimeStatus.IsActive())
                        {
                            // the last change result still doesn't report a final runtime status
                            // escalate the timeout by throwing an appropriate exception

                            throw new TimeoutException($"Provider '{provider.Id}' ran into timeout ({commandTimeout})");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                functionContext.SetCustomStatus($"Sending command failed: {exc.Message}", log, exc);

                commandResult ??= command.CreateResult();
                commandResult.Errors.Add(exc.AsSerializable());
            }
            finally
            {
                await functionContext
                    .AuditAsync(command, commandResult, provider)
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
                await functionContext
                    .AuditAsync(command, commandResult, provider)
                    .ConfigureAwait(true);

                functionContext.SetCustomStatus($"Switching command", log);

                var project = await functionContext
                    .GetProjectAsync(command.ProjectId, allowUnsafe: true)
                    .ConfigureAwait(true);

                functionContext.ContinueAsNew((
                    new ProviderProjectUpdateCommand
                    (
                        command.Api,
                        command.User as Model.Data.User,
                        project.PopulateExternalModel(),
                        command.CommandId),
                        provider
                    )
                );
            }
            catch (Exception exc)
            {
                functionContext.SetCustomStatus($"Switching command failed: {exc.Message}", log, exc);

                commandResult ??= command.CreateResult();
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }


        private static async Task ProcessOutputAsync(IDurableOrchestrationContext functionContext, Provider provider, IProviderCommand command, ICommandResult commandResult)
        {
            if (!string.IsNullOrEmpty(command.ProjectId) && commandResult is ICommandResult<ProviderOutput> providerOutputResult)
            {
                using (await functionContext.LockAsync<Project>(command.ProjectId).ConfigureAwait(true))
                {
                    var project = await functionContext
                        .GetProjectAsync(command.ProjectId)
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
