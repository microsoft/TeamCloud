/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Handlers
{
    public interface ICommandHandler
    {
        public bool CanHandle(ICommand orchestratorCommand, bool fallback = false)
        {
            if (orchestratorCommand is null)
                throw new ArgumentNullException(nameof(orchestratorCommand));

            return typeof(ICommandHandler<>)
                .MakeGenericType(orchestratorCommand.GetType())
                .IsAssignableFrom(GetType());
        }

        public Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient durableClient = null)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (CanHandle(command))
            {
                var handleMethod = typeof(ICommandHandler<>)
                    .MakeGenericType(command.GetType())
                    .GetMethod(nameof(HandleAsync), new Type[] { command.GetType(), typeof(IAsyncCollector<ICommand>), typeof(IDurableClient) });

                return (Task<ICommandResult>)handleMethod
                    .Invoke(this, new object[] { command, commandQueue, durableClient });
            }

            throw new NotImplementedException($"Missing orchestrator command handler implementation ICommandHandler<{command.GetType().Name}> at {GetType()}");
        }
    }

    public interface ICommandHandler<T> : ICommandHandler
        where T : class, ICommand
    {
        Task<ICommandResult> HandleAsync(T command, IAsyncCollector<ICommand> commandQueue, IDurableClient durableClient = null);
    }
}
