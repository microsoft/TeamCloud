/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Handlers;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Utilities
{
    internal static class OrchestratorCommandExtensions
    {
        internal static async Task<ICommand> GetCommandAsync(this IDurableClient durableClient, Guid commandId)
        {
            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var commandStatus = await durableClient
                .GetStatusAsync(CommandOrchestrationHandler.GetCommandOrchestrationWrapperInstanceId(commandId))
                .ConfigureAwait(false);

            return commandStatus?.Input?.HasValues ?? false
                ? commandStatus.Input.ToObject<ICommand>()
                : null;
        }

        internal static Task<ICommandResult> GetCommandResultAsync(this IDurableClient durableClient, ICommand command)
            => durableClient.GetCommandResultAsync(command?.CommandId ?? throw new ArgumentNullException(nameof(command)));

        internal static async Task<ICommandResult> GetCommandResultAsync(this IDurableClient durableClient, Guid commandId)
        {
            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var commandStatus = await durableClient
                .GetStatusAsync(CommandOrchestrationHandler.GetCommandOrchestrationInstanceId(commandId))
                .ConfigureAwait(false);

            if (commandStatus?.RuntimeStatus.IsFinal() ?? true)
            {
                // we use the command wrapper status as a fallback if there is no orchstration status available
                // for the command itself, but also if the command orchestration reached a final state and we
                // need to return the overall processing status (incl. all tasks managed by the wrapper)

                commandStatus = await durableClient
                    .GetStatusAsync(CommandOrchestrationHandler.GetCommandOrchestrationWrapperInstanceId(commandId))
                    .ConfigureAwait(false) ?? commandStatus; // final fallback if there is no wrapper orchstration
            }

            if (commandStatus != null)
            {
                var serializer = TeamCloudSerializerSettings.Default.CreateSerializer();

                ICommandResult commandResult;

                if (commandStatus.Output.HasValues)
                {
                    try
                    {
                        if (commandStatus.Output.SelectToken("$type") is null)
                        {
                            var command = commandStatus.Input
                                .ToObject<ICommand>(serializer);

                            var commandResultType = command.GetType()
                                .GetInterfaces()
                                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<,>))?
                                .GetGenericArguments().Last();

                            if (commandResultType is null)
                                throw new NotSupportedException($"Unable to deserialze command result json without type information: {commandStatus.Output}");

                            commandResult = (ICommandResult)commandStatus.Output
                                .ToObject(commandResultType, serializer);
                        }
                        else
                        {
                            commandResult = commandStatus.Output
                                .ToObject<ICommandResult>(serializer);
                        }
                    }
                    catch (Exception exc)
                    {
                        throw;
                    }
                }
                else
                {
                    // fallback to the default command result

                    commandResult = commandStatus.Input
                        .ToObject<ICommand>(serializer)
                        .CreateResult();
                }

                return commandResult.ApplyStatus(commandStatus);
            }

            return null;
        }
    }
}
