/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities
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

            try
            {
                var commandResult = context.GetInput<Input>().CommandResult;

                var commandStatus = await durableClient
                    .GetStatusAsync(commandResult.CommandId.ToString())
                    .ConfigureAwait(false);

                if (commandStatus != null)
                {
                    commandResult.ApplyStatus(commandStatus);

                    Debug.WriteLine($"Augmented command result: {TeamCloudSerialize.SerializeObject(commandResult)}");
                }

                return commandResult;
            }
            catch (Exception exc)
            {
                log?.LogError(exc, $"Failed to augment command result with orchestration status: {exc.Message}");

                throw exc.AsSerializable();
            }
        }

        internal struct Input
        {
            public ICommandResult CommandResult { get; set; }
        }
    }
}
