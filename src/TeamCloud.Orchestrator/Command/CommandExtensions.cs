/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command;

internal static class CommandExtensions
{
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
