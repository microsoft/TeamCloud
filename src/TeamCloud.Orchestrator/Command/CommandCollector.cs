using Microsoft.Azure.WebJobs;
using System;
using System.Threading;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command
{
    public sealed class CommandCollector : IAsyncCollector<ICommand>
    {
        private readonly IAsyncCollector<ICommand> collector;
        private readonly ICommand command;

        public CommandCollector(IAsyncCollector<ICommand> collector, ICommand command)
        {
            this.collector = collector ?? throw new ArgumentNullException(nameof(collector));
            this.command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public Task AddAsync(ICommand item, CancellationToken cancellationToken = default)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (!command.User.Id.Equals(item.User?.Id, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Command user must not different to base command ({command.User.Id})");

            item.ParentId = command.CommandId;

            return collector.AddAsync(item, cancellationToken);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
            => collector.FlushAsync(cancellationToken);
    }
}
