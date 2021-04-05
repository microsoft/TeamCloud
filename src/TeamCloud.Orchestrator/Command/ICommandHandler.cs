/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command
{
    public interface ICommandHandler
    {
        public const string ProcessorQueue = "command-processor";
        public const string MonitorQueue = "command-monitor";

        public bool Orchestration { get; }

        public bool CanHandle(ICommand command);

        public Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log);
    }

    public interface ICommandHandler<T> : ICommandHandler
        where T : class, ICommand
    {
        Task<ICommandResult> HandleAsync(T command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log);
    }
}
