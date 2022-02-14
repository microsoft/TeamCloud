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
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ProjectDeleteCommandHander : CommandHandler<ProjectDeleteCommand>
{
    private readonly IProjectRepository projectRepository;
    private readonly IComponentRepository componentRepository;

    public ProjectDeleteCommandHander(IProjectRepository projectRepository, IComponentRepository componentRepository)
    {
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
    }

    public override bool Orchestration => false;

    public override async Task<ICommandResult> HandleAsync(ProjectDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await projectRepository
                .RemoveAsync(command.Payload)
                .ConfigureAwait(false);

            var tasks = await componentRepository
                .ListAsync(commandResult.Result.Id)
                .Select(component => commandQueue.AddAsync(new ComponentDeleteCommand(command.User, component)))
                .ToListAsync()
                .ConfigureAwait(false);

            tasks.Add(commandQueue.AddAsync(new ProjectDestroyCommand(command.User, commandResult.Result)));

            await tasks
                .WhenAll()
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
