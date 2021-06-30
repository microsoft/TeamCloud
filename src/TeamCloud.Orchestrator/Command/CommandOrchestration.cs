/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestrator.Command.Activities;

namespace TeamCloud.Orchestrator.Command

{
    public sealed class CommandOrchestration
    {
        private readonly ICommandHandler[] commandHandlers;

        public CommandOrchestration(ICommandHandler[] commandHandlers)
        {
            this.commandHandlers = commandHandlers;
        }

        [FunctionName(nameof(CommandOrchestration))]
        public async Task Execute(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestratorContext,
            [DurableClient] IDurableClient orchestratorClient,
            [Queue(CommandHandler.ProcessorQueue)] IAsyncCollector<ICommand> commandQueue,
            ILogger log)
        {
            if (orchestratorClient is null)
                throw new ArgumentNullException(nameof(orchestratorClient));

            if (orchestratorContext is null)
                throw new ArgumentNullException(nameof(orchestratorContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = orchestratorContext.GetInput<ICommand>();
            var commandResult = command.CreateResult();

            try
            {
                await orchestratorContext
                    .AuditAsync(command, commandResult)
                    .ConfigureAwait(true);

                var commandHandler = commandHandlers
                    .SingleOrDefault(ch => ch.Orchestration && ch.CanHandle(command));

                if (commandHandler is null)
                    throw new NullReferenceException($"Could not find orchestration handler for command '{command.GetType()}'");

                await orchestratorContext
                    .AuditAsync(command, commandResult)
                    .ConfigureAwait(true);

                commandResult = await commandHandler
                    .HandleAsync(command, new CommandCollector(commandQueue, command, orchestratorContext), orchestratorClient, orchestratorContext, log)
                    .ConfigureAwait(true);

                if (commandResult is null)
                    throw new NullReferenceException($"Command handler '{commandHandler.GetType()}' returned NULL result");
            }
            catch (Exception exc)
            {
                commandResult ??= command.CreateResult();
                commandResult.Errors.Add(exc);
            }
            finally
            {
                await orchestratorContext
                    .AuditAsync(command, commandResult)
                    .ConfigureAwait(true);

                orchestratorContext.SetOutput(commandResult);
            }
        }
    }
}
