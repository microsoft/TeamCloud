/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public static class CommandResultActivity
    {
        [FunctionName(nameof(CommandResultActivity))]
        public static async Task<ICommandResult> RunActivity(
            [ActivityTrigger] IDurableActivityContext context,
            [DurableClient] IDurableClient durableClient,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            ICommandResult commandResult;

            try
            {
                commandResult = context.GetInput<ICommandResult>();
            }
            catch (Exception exc)
            {
                log?.LogError(exc, $"Failed deserialize command result from json: {exc.Message}");

                throw exc.AsSerializable();
            }

            try
            {
                var commandStatus = await durableClient
                    .GetStatusAsync(commandResult.CommandId.ToString())
                    .ConfigureAwait(false);

                if (commandStatus != null)
                    commandResult.ApplyStatus(commandStatus);
            }
            catch (Exception exc)
            {
                log?.LogWarning(exc, $"Failed to augment command result with orchestration status {commandResult.CommandId}: {exc.Message}");
            }

            return commandResult;
        }
    }
}
