/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ComponentTaskCreateCommandHandler : CommandHandler<ComponentTaskCreateCommand>
    {
        private readonly IComponentTaskRepository componentTaskRepository;

        public ComponentTaskCreateCommandHandler(IComponentTaskRepository componentTaskRepository)
        {
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        }

        public override bool Orchestration => false;

        public override async Task<ICommandResult> HandleAsync(ComponentTaskCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await componentTaskRepository
                    .AddAsync(command.Payload)
                    .ConfigureAwait(false);

                await commandQueue
                    .AddAsync(new ComponentTaskRunCommand(command.User, commandResult.Result))
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }
    }
}
