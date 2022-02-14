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

namespace TeamCloud.Orchestrator.Command;

public sealed class CommandCollector : IAsyncCollector<ICommand>
{
    private readonly IAsyncCollector<ICommand> collector;
    private readonly ICommand commandContext;
    private readonly IDurableOrchestrationContext orchestrationContext;

    public CommandCollector(IAsyncCollector<ICommand> collector, ICommand commandContext = null)
    {
        this.collector = collector ?? throw new ArgumentNullException(nameof(collector));
        this.commandContext = commandContext;
    }
    public CommandCollector(IDurableOrchestrationContext orchestrationContext, ICommand commandContext = null)
    {
        this.orchestrationContext = orchestrationContext ?? throw new ArgumentNullException(nameof(orchestrationContext));
        this.commandContext = commandContext;
    }

    public async Task AddAsync(ICommand item, CancellationToken cancellationToken = default)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        item.ParentId = commandContext?.CommandId ?? Guid.Empty;

        if (collector is not null)
        {
            await collector
                .AddAsync(item, cancellationToken)
                .ConfigureAwait(false);
        }
        else if (orchestrationContext is not null)
        {
            await orchestrationContext
                .CallActivityAsync(nameof(CommandEnqueueActivity), new CommandEnqueueActivity.Input() { Command = item })
                .ConfigureAwait(true);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public Task FlushAsync(CancellationToken cancellationToken = default)
        => collector.FlushAsync(cancellationToken);
}
