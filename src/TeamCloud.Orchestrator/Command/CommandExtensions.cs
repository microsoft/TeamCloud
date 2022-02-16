/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Command.Activities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command;

internal static class CommandExtensions
{
    internal static Task TerminateCommandAsync(this IDurableOrchestrationContext orchestrationContext, ComponentTask componentTask, string reason = null)
        => orchestrationContext.TerminateCommandAsync(Guid.Parse(componentTask.Id), reason);

    internal static Task TerminateCommandAsync(this IDurableOrchestrationContext orchestrationContext, ICommand command, string reason = null)
        => orchestrationContext.TerminateCommandAsync(command.CommandId, reason);

    internal static Task TerminateCommandAsync(this IDurableOrchestrationContext orchestrationContext, Guid commandId, string reason = null)
        => orchestrationContext.CallActivityAsync(nameof(CommandTerminateActivity), new CommandTerminateActivity.Input() { CommandId = commandId, Reason = reason });

    internal static Task<DurableOrchestrationStatus> GetCommandStatusAsync(this IDurableOrchestrationContext orchestrationContext, ComponentTask componentTask, bool showHistory = false, bool showHistoryOutput = false, bool showInput = true)
        => orchestrationContext.GetCommandStatusAsync(Guid.Parse(componentTask.Id), showHistory, showHistoryOutput, showInput);

    internal static Task<DurableOrchestrationStatus> GetCommandStatusAsync(this IDurableOrchestrationContext orchestrationContext, ICommand command, bool showHistory = false, bool showHistoryOutput = false, bool showInput = true)
        => orchestrationContext.GetCommandStatusAsync(command.CommandId, showHistory, showHistoryOutput, showInput);

    internal static Task<DurableOrchestrationStatus> GetCommandStatusAsync(this IDurableOrchestrationContext orchestrationContext, Guid commandId, bool showHistory = false, bool showHistoryOutput = false, bool showInput = true)
        => orchestrationContext.CallActivityAsync<DurableOrchestrationStatus>(nameof(CommandStatusActivity), new CommandStatusActivity.Input() { CommandId = commandId, ShowHistory = showHistory, ShowHistoryOutput = showHistoryOutput, ShowInput = showInput });

    internal static async Task<ICommand> GetCommandAsync(this IDurableClient durableClient, Guid commandId)
    {
        if (durableClient is null)
            throw new ArgumentNullException(nameof(durableClient));

        var commandStatus = await durableClient
            .GetStatusAsync(commandId.ToString())
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
            .GetStatusAsync(commandId.ToString())
            .ConfigureAwait(false);

        if (commandStatus is null)
            return null;

        var serializer = TeamCloudSerializerSettings.Default.CreateSerializer();

        var command = commandStatus.Input.HasValues
            ? commandStatus.Input.ToObject<ICommand>(serializer)
            : throw new NotSupportedException($"Unable to deserialize command: {commandStatus.Input}");

        var commandResult = command.CreateResult();

        if (commandStatus.Output.HasValues)
        {
            if (commandStatus.Output.SelectToken("$type") is not null)
            {
                commandResult = commandStatus.Output
                    .ToObject<ICommandResult>(serializer);
            }
            else
            {
                commandResult = (ICommandResult)commandStatus.Output
                    .ToObject(commandResult.GetType(), serializer);
            }
        }

        return commandResult.ApplyStatus(commandStatus);
    }
}
