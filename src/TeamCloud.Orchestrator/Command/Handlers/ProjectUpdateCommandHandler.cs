/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ProjectUpdateCommandHandler : CommandHandler<ProjectUpdateCommand>
{
    private readonly IProjectRepository projectRepository;

    public ProjectUpdateCommandHandler(IProjectRepository projectRepository)
    {
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
    }

    public override bool Orchestration => false;

    public override async Task<ICommandResult> HandleAsync(ProjectUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await projectRepository
                .SetAsync(command.Payload)
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
