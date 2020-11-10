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
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

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
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger logger)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            try
            {
                var functionInput = activityContext.GetInput<Input>();

                await commandAuditWriter
                    .AuditAsync(functionInput.Command, functionInput.CommandResult, functionInput.ProviderId)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                logger.LogError(exc, $"Command auditing failed: {exc.Message}");
            }
        }

        internal struct Input
        {
            public ICommand Command { get; set; }

            public ICommandResult CommandResult { get; set; }

            public string ProviderId { get; set; }
        }
    }

    internal static class CommandAuditExtensions
    {
        internal static Task AuditAsync(this IDurableOrchestrationContext orchestrationContext, ICommand command, ICommandResult commandResult = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return orchestrationContext.CallActivityWithRetryAsync(nameof(CommandAuditActivity), new CommandAuditActivity.Input
            {
                Command = command,
                CommandResult = commandResult
            });
        }
    }
}
