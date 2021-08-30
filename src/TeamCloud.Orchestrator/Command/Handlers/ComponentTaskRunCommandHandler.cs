/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Command.Activities.Adapters;
using TeamCloud.Orchestrator.Command.Activities.Components;
using TeamCloud.Orchestrator.Command.Activities.ComponentTasks;
using TeamCloud.Orchestrator.Command.Activities.Organizations;
using TeamCloud.Orchestrator.Command.Activities.Projects;
using TeamCloud.Orchestrator.Command.Entities;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ComponentTaskRunCommandHandler : CommandHandler<ComponentTaskRunCommand>
    {
        public ComponentTaskRunCommandHandler() : base(true)
        { }

        public override async Task<ICommandResult> HandleAsync(ComponentTaskRunCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)

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

            var organization = await WaitForOrganizationInitAsync(orchestrationContext, command)
                .ConfigureAwait(true);
            
            var project = await WaitForProjectInitAsync(orchestrationContext, command)
                .ConfigureAwait(true);

            var component = await orchestrationContext
                .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.ComponentId, ProjectId = command.Payload.ProjectId })
                .ConfigureAwait(true);

            using (await orchestrationContext.LockContainerDocumentAsync(component).ConfigureAwait(true))
            {
                try
                {
                    commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, TaskState.Initializing)
                        .ConfigureAwait(true);

                    if (!AzureResourceIdentifier.TryParse(component.IdentityId, out var _))
                    {
                        // ensure every component has an identity assigned that can be used
                        // as the identity of the task runner container to access azure or
                        // call back into teamcloud using the azure cli extension

                        component = await orchestrationContext
                            .CallActivityWithRetryAsync<Component>(nameof(ComponentIdentityActivity), new ComponentIdentityActivity.Input() { Component = component })
                            .ConfigureAwait(true);
                    }

                    if (command.Payload.Type == ComponentTaskType.Create)
                    {
                        try
                        {
                            component = await UpdateComponentAsync(component, ResourceState.Provisioning)
                                .ConfigureAwait(true);

                            component = await orchestrationContext
                                .CallActivityWithRetryAsync<Component>(nameof(AdapterCreateComponentActivity), new AdapterCreateComponentActivity.Input() { Component = component, User = command.User })
                                .ConfigureAwait(true);

                            component = await UpdateComponentAsync(component, ResourceState.Provisioned)
                                .ConfigureAwait(true);
                        }
                        catch
                        {
                            component = await UpdateComponentAsync(component, ResourceState.Failed)
                                .ConfigureAwait(true);

                            throw;
                        }
                    }

                    if (component.ResourceState == ResourceState.Provisioned || command.Payload.Type == ComponentTaskType.Delete)
                    {
                        // lets do the dirty work and run the script associated with the current component task using our container based task runner infrasturcture

                        commandResult.Result = await orchestrationContext
                            .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskRunnerActivity), new ComponentTaskRunnerActivity.Input() { ComponentTask = commandResult.Result })
                            .ConfigureAwait(true);

                        // as we don't control the script execution itself we need to monitor/poll the state of the task runner container instance until it reaches a final state

                        while (!commandResult.Result.TaskState.IsFinal())
                        {
                            if (commandResult.Result.Created.AddMinutes(30) < orchestrationContext.CurrentUtcDateTime)
                                throw new TimeoutException($"Maximum task execution time (30min) exceeded.");

                            await orchestrationContext
                                .CreateTimer(TimeSpan.FromSeconds(2))
                                .ConfigureAwait(true);

                            commandResult.Result = await orchestrationContext
                                .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentDeploymentMonitorActivity), new ComponentDeploymentMonitorActivity.Input() { ComponentTask = commandResult.Result })
                                .ConfigureAwait(true);
                        }
                    }
                    else if (command.Payload.Type != ComponentTaskType.Delete)
                    {
                        // all command task types except delete require a successfully provisioned and healty component - if not given we throw an exception

                        throw new NotSupportedException($"Unable to process {command.CommandAction} task on component {component} when in state {component.ResourceState}");
                    }

                    if (command.Payload.Type == ComponentTaskType.Delete)
                    {
                        try
                        {
                            component = await UpdateComponentAsync(component, ResourceState.Deprovisioning)
                                .ConfigureAwait(true);

                            component = await orchestrationContext
                                .CallActivityWithRetryAsync<Component>(nameof(AdapterDeleteComponentActivity), new AdapterDeleteComponentActivity.Input() { Component = component, User = command.User })
                                .ConfigureAwait(true);

                            component = await UpdateComponentAsync(component, ResourceState.Deprovisioned)
                                .ConfigureAwait(true);
                        }
                        catch
                        {
                            component = await UpdateComponentAsync(component, ResourceState.Failed)
                                .ConfigureAwait(true);

                            throw;
                        }
                    }
                    else
                    {
                        await commandQueue
                            .AddAsync(new ComponentUpdateCommand(command.User, component))
                            .ConfigureAwait(true);
                    }

                    if (commandResult.Result.ExitCode.GetValueOrDefault(0) == 0)
                    {
                        // if no exit code was provided by the task runner we assume success
                        commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, TaskState.Succeeded)
                            .ConfigureAwait(true);
                    }
                    else
                    {
                        // the task runner returned with an exit code other the 0 (zero)
                        commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, TaskState.Failed)
                            .ConfigureAwait(true);
                    }
                }
                catch
                {
                    commandResult.Result = await UpdateComponentTaskAsync(commandResult.Result, TaskState.Failed)
                        .ConfigureAwait(true);

                    throw;
                }
                finally
                {

                    switch (commandResult.Result.Type)
                    {
                        case ComponentTaskType.Create:

                            if (component.ResourceState == ResourceState.Failed)
                            {
                                // component provisioning failed - let's initiate a delete task to break down the related resource

                                await commandQueue
                                    .AddAsync(new ComponentDeleteCommand(command.User, component))
                                    .ConfigureAwait(true);
                            }

                            break;

                        case ComponentTaskType.Delete:

                            if (component.ResourceState == ResourceState.Deprovisioned)
                            {
                                // the component was successfully deprovisioned - let's get rid of the component itself
                            
                                await orchestrationContext
                                    .CallActivityWithRetryAsync(nameof(ComponentRemoveActivity), new ComponentRemoveActivity.Input() { ComponentId = component.Id, ProjectId = component.ProjectId })
                                    .ConfigureAwait(true);
                            }
                            else if (component.Deleted.HasValue)
                            {
                                // deprovisioning failed for whatever reason - remove the deleted flag to do some cleanup work

                                component.Deleted = null;

                                _ = await UpdateComponentAsync(component).ConfigureAwait(true);
                            }

                            break;

                    }

                    // finally do some cleanup work and get rid of the component task runner if exists

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

            Task<ComponentTask> UpdateComponentTaskAsync(ComponentTask componentTask, TaskState? taskState = null)
            {
                componentTask.TaskState = taskState.GetValueOrDefault(componentTask.TaskState);

                return orchestrationContext.CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskSetActivity), new ComponentTaskSetActivity.Input() { ComponentTask = componentTask });
            }
        }

        private static async Task<Project> WaitForProjectInitAsync(IDurableOrchestrationContext orchestrationContext, ComponentTaskRunCommand command)
        {
            var project = await orchestrationContext
                .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Organization = command.OrganizationId, Id = command.ProjectId })
                .ConfigureAwait(true);

            if (!project.ResourceState.IsFinal())
            {
                using (await orchestrationContext.LockContainerDocumentAsync(project).ConfigureAwait(true))
                {
                    project = project.ResourceState == ResourceState.Provisioned ? project : await orchestrationContext
                        .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Organization = command.OrganizationId, Id = command.ProjectId })
                        .ConfigureAwait(true);

                    if (!project.ResourceState.IsFinal())
                    {
                        orchestrationContext.ContinueAsNew(command, false);
                    }
                    else if (project.ResourceState == ResourceState.Failed)
                    {
                        throw new NotSupportedException("Can't process task when project resource state is 'Failed'");
                    }
                }
            }

            return project;
        }

        private static async Task<Organization> WaitForOrganizationInitAsync(IDurableOrchestrationContext orchestrationContext, ComponentTaskRunCommand command)
        {
            var organization = await orchestrationContext
                .CallActivityWithRetryAsync<Organization>(nameof(OrganizationGetActivity), new OrganizationGetActivity.Input() { Id = command.OrganizationId })
                .ConfigureAwait(true);

            if (!organization.ResourceState.IsFinal())
            {
                using (await orchestrationContext.LockContainerDocumentAsync(organization).ConfigureAwait(true))
                {
                    organization = organization.ResourceState == ResourceState.Provisioned ? organization : await orchestrationContext
                        .CallActivityWithRetryAsync<Organization>(nameof(OrganizationGetActivity), new OrganizationGetActivity.Input() { Id = command.OrganizationId })
                        .ConfigureAwait(true);

                    if (!organization.ResourceState.IsFinal())
                    {
                        orchestrationContext.ContinueAsNew(command, false);
                    }
                    else if (organization.ResourceState == ResourceState.Failed)
                    {
                        throw new NotSupportedException("Can't process task when organization resource state is 'Failed'");
                    }
                }
            }

            return organization;
        }
    }
}
