/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Audit;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestrator.Operations.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.API
{
    public sealed class MonitorTrigger
    {
        public const string CommandMonitorQueue = "Command-Monitor";

        private readonly ICommandAuditWriter commandAuditWriter;

        public MonitorTrigger(ICommandAuditWriter commandAuditWriter)
        {
            this.commandAuditWriter = commandAuditWriter ?? throw new ArgumentNullException(nameof(commandAuditWriter));
        }

        [FunctionName(nameof(MonitorTrigger) + nameof(CommandMonitorQueue))]
        public async Task RunCommandMonitorQueue(
            [QueueTrigger(CommandMonitorQueue)] CloudQueueMessage commandMessage,
            [Queue(CommandMonitorQueue)] CloudQueue commandMonitor,
            [DurableClient] IDurableClient durableClient,
            ILogger log)
        {
            if (commandMessage is null)
                throw new ArgumentNullException(nameof(commandMessage));

            if (commandMonitor is null)
                throw new ArgumentNullException(nameof(commandMonitor));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            log ??= NullLogger.Instance;

            // there is no error handler on purpose - this way we can leverage
            // the function runtime capabilities for poisened message queues
            // and don't need to handle this on our own.

            if (Guid.TryParse(commandMessage.AsString, out var commandId))
            {
                var command = await durableClient
                    .GetCommandAsync(commandId)
                    .ConfigureAwait(false);

                if (command is null)
                {
                    // we could find a command based on the enqueued command id - warn and forget

                    log.LogWarning($"Monitoring command failed: Could not find command {commandId}");
                }
                else
                {
                    var commandResult = await durableClient
                        .GetCommandResultAsync(commandId)
                        .ConfigureAwait(false);

                    await commandAuditWriter
                        .AuditAsync(command, commandResult)
                        .ConfigureAwait(false);

                    if (!(commandResult?.RuntimeStatus.IsFinal() ?? false))
                    {
                        // the command result is still not in a final state - as we want to monitor the command until it is done,
                        // we are going to re-enqueue the command ID with a visibility offset to delay the next result lookup.

                        await commandMonitor
                            .AddMessageAsync(new CloudQueueMessage(commandId.ToString()), null, TimeSpan.FromSeconds(10), null, null)
                            .ConfigureAwait(false);
                    }
                }
            }
            else
            {
                // we expect that the queue message is a valid guid (command ID) - warn and forget

                log.LogWarning($"Monitoring command failed: Invalid command ID ({commandMessage.AsString})");
            }
        }
    }
}
