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
using TeamCloud.Model.Handlers;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities
{
    public sealed class CommandCollectActivity
    {
        [FunctionName(nameof(CommandCollectActivity))]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            [Queue(CommandHandler.ProcessorQueue)] IAsyncCollector<ICommand> commandCollector,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            if (commandCollector is null)
                throw new ArgumentNullException(nameof(commandCollector));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                var input = activityContext.GetInput<Input>();

                await commandCollector
                    .AddAsync(input.Command)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Failed to collect command: {exc.Message}");

                throw exc.AsSerializable();
            }
        }

        internal struct Input
        {
            public ICommand Command { get; set; }
        }
    }
}
