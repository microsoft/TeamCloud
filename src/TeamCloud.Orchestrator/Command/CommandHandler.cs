/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command;

public abstract class CommandHandler<TCommand> : CommandHandler, ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    public abstract Task<ICommandResult> HandleAsync(TCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log);
}

public abstract class CommandHandler : ICommandHandler
{
    public const string ProcessorQueue = "command-processor";
    public const string MonitorQueue = "command-monitor";

    public abstract bool Orchestration { get; }

    public virtual bool CanHandle(ICommand command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        return typeof(ICommandHandler<>)
            .MakeGenericType(command.GetType())
            .IsAssignableFrom(GetType());
    }

    public virtual Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (CanHandle(command))
        {
            var handleMethod = typeof(ICommandHandler<>)
                .MakeGenericType(command.GetType())
                .GetMethod(nameof(HandleAsync), new Type[] { command.GetType(), typeof(IAsyncCollector<ICommand>), typeof(IDurableOrchestrationContext), typeof(ILogger) });

            return (Task<ICommandResult>)handleMethod
                .Invoke(this, new object[] { command, commandQueue, orchestrationContext, log });
        }

        throw new NotImplementedException($"Missing orchestrator command handler implementation ICommandHandler<{command.GetTypeName(prettyPrint: true)}> at {GetType()}");
    }
}
