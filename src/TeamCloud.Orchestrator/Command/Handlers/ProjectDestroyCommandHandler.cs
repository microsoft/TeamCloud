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
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Command.Activities.Projects;
using TeamCloud.Orchestrator.Command.Entities;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ProjectDestroyCommandHandler : CommandHandler<ProjectDestroyCommand>
{
    private readonly IComponentRepository componentRepository;

    public ProjectDestroyCommandHandler(IComponentRepository componentRepository)
    {
        this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
    }

    public override bool Orchestration => true;

    public override async Task<ICommandResult> HandleAsync(ProjectDestroyCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (orchestrationContext is null)
            throw new ArgumentNullException(nameof(orchestrationContext));

        if (log is null)
            throw new ArgumentNullException(nameof(log));

        var commandResult = command.CreateResult();

        using (await orchestrationContext.LockContainerDocumentAsync(command.Payload).ConfigureAwait(true))
        {
            // just to make sure we are dealing with the latest version
            // of the Project entity, we re-fetch the entity and
            // use the passed in one as a potential fallback.

            commandResult.Result = (await orchestrationContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Id = command.Payload.Id, Organization = command.Payload.Organization })
                .ConfigureAwait(true)) ?? command.Payload;

            if (commandResult.Result.Deleted.HasValue)
            {
                try
                {
                    commandResult.Result = await UpdateProjectAsync(commandResult.Result, ResourceState.Deprovisioning)
                        .ConfigureAwait(true);

                    var components = await componentRepository
                        .ListAsync(commandResult.Result.Id, includeDeleted: true)
                        .ToArrayAsync()
                        .ConfigureAwait(true);

                    if (components.Any(c => c.ResourceState.IsActive()))
                    {
                        // at least one of the component in the context of this
                        // project are in an active state. we postpone the project
                        // destroy operation until all components are gone or
                        // in a deprovisioned state.

                        await orchestrationContext
                            .ContinueAsNew(command, TimeSpan.FromMinutes(1))
                            .ConfigureAwait(true);
                    }
                    else if (components.Any(c => c.ResourceState != ResourceState.Deprovisioned))
                    {
                        // at least one of the components reached a final state
                        // other than deprovisioned. we simple cant destroy the
                        // project in this situation but need to switch back into
                        // a provisioned state to give to user to fix issues
                        // on the component level

                        commandResult.Result = await UpdateProjectAsync(commandResult.Result, ResourceState.Provisioned, restore: true)
                            .ConfigureAwait(true);
                    }
                    else
                    {
                        // we are good to delete project related resources and
                        // finally delete the project itself in our data store

                        await orchestrationContext
                            .CallActivityWithRetryAsync(nameof(ProjectDestroyActivity), new ProjectDestroyActivity.Input() { Project = commandResult.Result })
                            .ConfigureAwait(true);

                        commandResult.Result = await UpdateProjectAsync(commandResult.Result, ResourceState.Deprovisioned)
                            .ConfigureAwait(true);
                    }
                }
                catch
                {
                    commandResult.Result = await UpdateProjectAsync(commandResult.Result, ResourceState.Failed, restore: true)
                        .ConfigureAwait(true);

                    throw;
                }
            }
        }

        return commandResult;

        Task<Project> UpdateProjectAsync(Project project, ResourceState? resourceState = null, bool restore = false)
        {
            project.ResourceState = resourceState ?? project.ResourceState;

            if (restore)
            {
                project.Deleted = null;
                project.TTL = null;
            }

            return orchestrationContext.CallActivityWithRetryAsync<Project>(nameof(ProjectSetActivity), new ProjectSetActivity.Input() { Project = commandResult.Result, ResourceState = ResourceState.Deprovisioned });

        }
    }
}
