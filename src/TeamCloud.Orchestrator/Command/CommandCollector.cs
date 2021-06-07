/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestrator.Command.Activities;

namespace TeamCloud.Orchestrator.Command
{
    public sealed class CommandCollector : IAsyncCollector<ICommand>
    {
        private readonly IAsyncCollector<ICommand> collector;
        private readonly ICommand commandContext;
        private readonly IDurableOrchestrationContext orchestrationContext;

        public CommandCollector(IAsyncCollector<ICommand> collector, ICommand commandContext = null, IDurableOrchestrationContext orchestrationContext = null)
        {
            this.collector = collector ?? throw new ArgumentNullException(nameof(collector));
            this.commandContext = commandContext;
            this.orchestrationContext = orchestrationContext;
        }

        public async Task AddAsync(ICommand item, CancellationToken cancellationToken = default)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (commandContext != null)
            {
                if (commandContext.User.Id.Equals(item.User?.Id, StringComparison.OrdinalIgnoreCase))
                    item.ParentId = commandContext.CommandId;
                else
                    throw new ArgumentException($"Command user must not different to base command ({commandContext.User.Id})");
            }

            if (orchestrationContext is null)
            {
                await collector
                    .AddAsync(item, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await orchestrationContext
                    .CallActivityAsync(nameof(CommandCollectActivity), new CommandCollectActivity.Input() { Command = item })
                    .ConfigureAwait(true);
            }
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
            => collector.FlushAsync(cancellationToken);
    }
}
