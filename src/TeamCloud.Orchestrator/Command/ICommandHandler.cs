/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command;

public interface ICommandHandler
{
    bool Orchestration { get; }

    bool CanHandle(ICommand command);

    Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log);
}

public interface ICommandHandler<T> : ICommandHandler
    where T : class, ICommand
{
    Task<ICommandResult> HandleAsync(T command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log);
}
