/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Audit;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Activities
{
    public sealed class CommandAuditActivity
    {
        private readonly ICommandAuditWriter commandAuditWriter;

        public CommandAuditActivity(ICommandAuditWriter commandAuditWriter)
        {
            this.commandAuditWriter = commandAuditWriter ?? throw new ArgumentNullException(nameof(commandAuditWriter));
        }

        [FunctionName(nameof(CommandAuditActivity))]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger logger)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            try
            {
                var (command, commandResult, providerId) =
                    functionContext.GetInput<(ICommand, ICommandResult, string)>();

                await commandAuditWriter
                    .AuditAsync(command, commandResult, providerId)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                logger.LogError(exc, $"Command auditing failed: {exc.Message}");
            }
        }
    }
}
