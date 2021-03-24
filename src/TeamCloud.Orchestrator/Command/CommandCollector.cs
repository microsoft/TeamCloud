using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestrator.Command.Activities;

namespace TeamCloud.Orchestrator.Command
{
    public sealed class CommandCollector : IAsyncCollector<ICommand>
    {
        private readonly IAsyncCollector<ICommand> collector;
        private readonly ICommand command;
        private readonly IDurableOrchestrationContext context;

        public CommandCollector(IAsyncCollector<ICommand> collector, ICommand command, IDurableOrchestrationContext context = null)
        {
            this.collector = collector ?? throw new ArgumentNullException(nameof(collector));
            this.command = command ?? throw new ArgumentNullException(nameof(command));
            this.context = context;
        }

        public async Task AddAsync(ICommand item, CancellationToken cancellationToken = default)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (!command.User.Id.Equals(item.User?.Id, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Command user must not different to base command ({command.User.Id})");

            item.ParentId = command.CommandId;

            if (context is null)
            {
                await collector
                    .AddAsync(item, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await context
                    .CallActivityAsync(nameof(CommandCollectActivity), new CommandCollectActivity.Input() { Command = item })
                    .ConfigureAwait(true);
            }
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
            => collector.FlushAsync(cancellationToken);
    }
}
