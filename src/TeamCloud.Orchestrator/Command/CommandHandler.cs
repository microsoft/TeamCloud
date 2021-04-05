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

namespace TeamCloud.Orchestrator.Command
{
    public abstract class CommandHandler : ICommandHandler
    {
        public CommandHandler(bool orchestration = false)
        {
            Orchestration = orchestration;
        }

        public bool Orchestration { get; }

        public virtual bool CanHandle(ICommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return typeof(ICommandHandler<>)
                .MakeGenericType(command.GetType())
                .IsAssignableFrom(GetType());
        }

        public virtual Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (CanHandle(command))
            {
                var handleMethod = typeof(ICommandHandler<>)
                    .MakeGenericType(command.GetType())
                    .GetMethod(nameof(HandleAsync), new Type[] { command.GetType(), typeof(IAsyncCollector<ICommand>), typeof(IDurableClient), typeof(IDurableOrchestrationContext), typeof(ILogger) });

                return (Task<ICommandResult>)handleMethod
                    .Invoke(this, new object[] { command, commandQueue, orchestrationClient, orchestrationContext, log });
            }

            throw new NotImplementedException($"Missing orchestrator command handler implementation ICommandHandler<{command.GetType().Name}> at {GetType()}");
        }
    }
}
