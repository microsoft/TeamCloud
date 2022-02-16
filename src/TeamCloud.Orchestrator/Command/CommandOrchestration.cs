/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestrator.Command.Activities;

namespace TeamCloud.Orchestrator.Command;

public sealed class CommandOrchestration
{
    private readonly ICommandHandler[] commandHandlers;

    public CommandOrchestration(ICommandHandler[] commandHandlers)
    {
        this.commandHandlers = commandHandlers;
    }

    // [Deterministic]
    [FunctionName(nameof(CommandOrchestration))]
    public async Task Execute(
        [OrchestrationTrigger] IDurableOrchestrationContext orchestratorContext,
        ILogger log)
    {
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
                .HandleAsync(command, new CommandCollector(orchestratorContext, command), orchestratorContext, log)
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
