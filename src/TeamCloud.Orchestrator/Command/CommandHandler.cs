/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Jose;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command;

public abstract class CommandHandler<TCommand> : CommandHandler, ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    public abstract Task<ICommandResult> HandleAsync(TCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log);
}

public abstract class CommandHandler : ICommandHandler
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>> HandleMethodCache = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>>();

    private MethodInfo GetHandleMethod(ICommand command) => HandleMethodCache
        .GetOrAdd(GetType(), _ => new ConcurrentDictionary<Type, MethodInfo>())
        .GetOrAdd(command.GetType(), commandType =>
    {
        var handlerInterface = typeof(ICommandHandler<>)
            .MakeGenericType(commandType);

        if (handlerInterface.IsAssignableFrom(GetType()))
            return handlerInterface.GetMethod(nameof(HandleAsync), new Type[] { command.GetType(), typeof(IAsyncCollector<ICommand>), typeof(IDurableOrchestrationContext), typeof(ILogger) });

        return null;
    });

    public const string ProcessorQueue = "command-processor";
    public const string MonitorQueue = "command-monitor";

    public abstract bool Orchestration { get; }

    public virtual bool CanHandle(ICommand command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        return GetHandleMethod(command) is not null;
    }

    public virtual Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (CanHandle(command))
            return (Task<ICommandResult>)GetHandleMethod(command).Invoke(this, new object[] { command, commandQueue, orchestrationContext, log });

        throw new NotImplementedException($"Missing orchestrator command handler implementation ICommandHandler<{command.GetTypeName(prettyPrint: true)}> at {GetType()}");
    }
}
