/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Model.Handlers;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Command.Activities.Components;
using TeamCloud.Orchestrator.Command.Activities.ComponentTasks;
using TeamCloud.Orchestrator.Command.Activities.Organizations;
using TeamCloud.Orchestrator.Command.Activities.Projects;
using TeamCloud.Orchestrator.Command.Entities;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ComponentTaskRunCommandHandler : CommandHandler,
        ICommandHandler<ComponentTaskRunCommand>
    {
        public ComponentTaskRunCommandHandler() : base(true)
        { }

        public async Task<ICommandResult> HandleAsync(ComponentTaskRunCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)

        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var commandResult = command.CreateResult();

            commandResult.Result = await orchestrationContext
                .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskGetActivity), new ComponentTaskGetActivity.Input() { ComponentTaskId = command.Payload.Id, ComponentId = command.Payload.ComponentId })
                .ConfigureAwait(true) ?? command.Payload;

            var organization = await orchestrationContext
                .CallActivityWithRetryAsync<Organization>(nameof(OrganizationGetActivity), new OrganizationGetActivity.Input() { Id = commandResult.Result.Organization })
                .ConfigureAwait(true);

            using (await orchestrationContext.LockContainerDocumentAsync(organization).ConfigureAwait(true))
            {
                organization = organization.ResourceState == ResourceState.Succeeded ? organization : await orchestrationContext
                    .CallActivityWithRetryAsync<Organization>(nameof(OrganizationGetActivity), new OrganizationGetActivity.Input() { Id = commandResult.Result.Organization })
                    .ConfigureAwait(true);

                if (!organization.ResourceState.IsFinal())
                    orchestrationContext.ContinueAsNew(command, false);
                else if (organization.ResourceState == ResourceState.Failed)
                    throw new NotSupportedException("Can't process task when organization resource state is 'Failed'");
            }

            var project = await orchestrationContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Organization = commandResult.Result.Organization, Id = commandResult.Result.ProjectId })
                .ConfigureAwait(true);

            using (await orchestrationContext.LockContainerDocumentAsync(project).ConfigureAwait(true))
            {
                project = project.ResourceState == ResourceState.Succeeded ? project : await orchestrationContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Organization = commandResult.Result.Organization, Id = commandResult.Result.ProjectId })
                    .ConfigureAwait(true);

                if (!project.ResourceState.IsFinal())
                    orchestrationContext.ContinueAsNew(command, false);
                else if (project.ResourceState == ResourceState.Failed)
                    throw new NotSupportedException("Can't process task when project resource state is 'Failed'");
            }

            var component = await orchestrationContext
                .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.ComponentId, ProjectId = command.Payload.ProjectId })
                .ConfigureAwait(true);

            using (await orchestrationContext.LockContainerDocumentAsync(component).ConfigureAwait(true))
            {
                try
                {
                    commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, ResourceState.Initializing)
                        .ConfigureAwait(true);

                    try
                    {
                        if (component.ResourceState != ResourceState.Succeeded)
                        {
                            // we only switch to a provisioning resource state if the component wasn't
                            // in a successful resource state before. just to avoid the confusion on the
                            // FE side if the an already successful provisioned component goes back
                            // into provisionine state.

                            component = await UpdateComponentAsync(component, ResourceState.Provisioning)
                                .ConfigureAwait(true);
                        }

                        component = await orchestrationContext
                            .CallActivityWithRetryAsync<Component>(nameof(ComponentEnsureIdentityActivity), new ComponentEnsureIdentityActivity.Input() { Component = component })
                            .ConfigureAwait(true);

                        component = await orchestrationContext
                            .CallActivityWithRetryAsync<Component>(nameof(ComponentEnsureContainerActivity), new ComponentEnsureContainerActivity.Input() { Component = component })
                            .ConfigureAwait(true);

                        component = await UpdateComponentAsync(component, ResourceState.Succeeded)
                            .ConfigureAwait(true);
                    }
                    catch
                    {
                        component = await UpdateComponentAsync(component, ResourceState.Failed)
                            .ConfigureAwait(true);

                        throw;
                    }
                    finally
                    {
                        await commandQueue
                            .AddAsync(new ComponentUpdateCommand(command.User, component))
                            .ConfigureAwait(true);
                    }

                    commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, ResourceState.Provisioning)
                        .ConfigureAwait(true);

                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskRunnerActivity), new ComponentTaskRunnerActivity.Input() { ComponentTask = commandResult.Result })
                        .ConfigureAwait(true);

                    while (!commandResult.Result.ResourceState.IsFinal())
                    {
                        if (commandResult.Result.Created.AddMinutes(30) < orchestrationContext.CurrentUtcDateTime)
                            throw new TimeoutException($"Maximum task execution time (30min) exceeded.");

                        await orchestrationContext
                            .CreateTimer(TimeSpan.FromSeconds(3))
                            .ConfigureAwait(true);

                        commandResult.Result = await orchestrationContext
                            .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentDeploymentMonitorActivity), new ComponentDeploymentMonitorActivity.Input() { ComponentTask = commandResult.Result })
                            .ConfigureAwait(true);
                    }

                    if (commandResult.Result.ExitCode.GetValueOrDefault(0) == 0)
                    {
                        // if no exit code was provided by the task runner we assume success
                        commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, ResourceState.Succeeded)
                            .ConfigureAwait(true);
                    }
                    else
                    {
                        // the task runner returned with an exit code other the 0 (zero)
                        commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, ResourceState.Failed)
                            .ConfigureAwait(true);
                    }
                }
                catch
                {
                    commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, ResourceState.Failed)
                        .ConfigureAwait(true);

                    throw;
                }
                finally
                {
                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskTerminateActivity), new ComponentTaskTerminateActivity.Input() { ComponentTask = commandResult.Result })
                        .ConfigureAwait(true);
                }
            }

            return commandResult;

            Task<Component> UpdateComponentAsync(Component component, ResourceState? resourceState = null)
            {
                component.ResourceState = resourceState.GetValueOrDefault(component.ResourceState);

                return orchestrationContext.CallActivityWithRetryAsync<Component>(nameof(ComponentSetActivity), new ComponentSetActivity.Input() { Component = component });
            }

            Task<ComponentTask> UpdateComponentTaskAsync(ComponentTask componentTask, ResourceState? resourceState = null)
            {
                componentTask.ResourceState = resourceState.GetValueOrDefault(componentTask.ResourceState);

                return orchestrationContext.CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskSetActivity), new ComponentTaskSetActivity.Input() { ComponentTask = componentTask });
            }
        }
    }
}
