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

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ComponentTaskRunCommandHandler : CommandHandler<ComponentTaskRunCommand>
{
    public override bool Orchestration => true;

    public override async Task<ICommandResult> HandleAsync(ComponentTaskRunCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)

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

        var organization = await WaitForOrganizationInitAsync(orchestrationContext, command)
            .ConfigureAwait(true);

        var project = await WaitForProjectInitAsync(orchestrationContext, command)
            .ConfigureAwait(true);

        var component = await orchestrationContext
            .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ComponentId = command.Payload.ComponentId, ProjectId = command.Payload.ProjectId })
            .ConfigureAwait(true);

        using (await orchestrationContext.LockContainerDocumentAsync(component).ConfigureAwait(true))
        {
            commandResult.Result = await orchestrationContext
                .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskGetActivity), new ComponentTaskGetActivity.Input() { ComponentTaskId = command.Payload.Id, ComponentId = command.Payload.ComponentId })
                .ConfigureAwait(true) ?? command.Payload;

            if (commandResult.Result.TaskState != TaskState.Canceled)
            {
                try
                {
                    commandResult.Result = await UpdateComponentTaskAsync(orchestrationContext, commandResult.Result, TaskState.Initializing)
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

                    commandResult.Result = await UpdateComponentTaskAsync(orchestrationContext, commandResult.Result, TaskState.Processing)
                        .ConfigureAwait(true);

                    await (command.Payload.Type switch
                    {
                        ComponentTaskType.Create => ProcessCreateCommandAsync(),
                        ComponentTaskType.Delete => ProcessDeleteCommandAsync(),
                        ComponentTaskType.Custom => ProcessCustomCommandAsync(),

                        _ => throw new NotSupportedException($"The command type '{command.Payload.Type}' is not supported")

                    }).ConfigureAwait(true);

                    commandResult.Result = await UpdateComponentTaskAsync(orchestrationContext, commandResult.Result, TaskState.Succeeded)
                        .ConfigureAwait(true);
                }
                catch
                {
                    commandResult.Result = await UpdateComponentTaskAsync(orchestrationContext, commandResult.Result, TaskState.Failed)
                        .ConfigureAwait(true);

                    throw;
                }
                finally
                {
                    // finally do some cleanup work and get rid of the component task runner if exists

                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskTerminateActivity), new ComponentTaskTerminateActivity.Input() { ComponentTask = commandResult.Result })
                        .ConfigureAwait(true);
                }
            }
        }

        return commandResult;

        async Task ProcessCreateCommandAsync()
        {
            try
            {
                component = await UpdateComponentAsync(orchestrationContext, component, ResourceState.Provisioning)
                    .ConfigureAwait(true);

                component = await orchestrationContext
                    .CallActivityWithRetryAsync<Component>(nameof(AdapterCreateComponentActivity), new AdapterCreateComponentActivity.Input() { Component = component, User = command.User })
                    .ConfigureAwait(true);

                await RunComponentTaskAsync()
                    .ConfigureAwait(true);

                component = await UpdateComponentAsync(orchestrationContext, component, ResourceState.Provisioned)
                    .ConfigureAwait(true);

                await commandQueue
                    .AddAsync(new ComponentUpdateCommand(command.User, component))
                    .ConfigureAwait(true);

            }
            catch
            {
                // component provisioning failed - let's initiate a delete task to break down the related resource

                await commandQueue
                    .AddAsync(new ComponentDeleteCommand(command.User, component))
                    .ConfigureAwait(true);

                await UpdateComponentAsync(orchestrationContext, component, ResourceState.Failed)
                    .ConfigureAwait(true);

                throw;
            }
        }

        async Task ProcessDeleteCommandAsync()
        {
            try
            {
                component = await UpdateComponentAsync(orchestrationContext, component, ResourceState.Deprovisioning)
                    .ConfigureAwait(true);

                await RunComponentTaskAsync()
                    .ConfigureAwait(true);

                component = await orchestrationContext
                    .CallActivityWithRetryAsync<Component>(nameof(AdapterDeleteComponentActivity), new AdapterDeleteComponentActivity.Input() { Component = component, User = command.User })
                    .ConfigureAwait(true);

                component = await UpdateComponentAsync(orchestrationContext, component, ResourceState.Deprovisioned)
                    .ConfigureAwait(true);


                await orchestrationContext
                    .CallActivityWithRetryAsync(nameof(ComponentRemoveActivity), new ComponentRemoveActivity.Input() { ComponentId = component.Id, ProjectId = component.ProjectId })
                    .ConfigureAwait(true);
            }
            catch
            {
                component.Deleted = null;
                component.TTL = null;

                component = await UpdateComponentAsync(orchestrationContext, component, ResourceState.Provisioned)
                    .ConfigureAwait(true);

                throw;
            }
        }

        async Task ProcessCustomCommandAsync()
        {
            if (component.ResourceState != ResourceState.Provisioned)
                throw new NotSupportedException($"Unable to process task '{commandResult.Result.TypeName}' when component is in state '{component.ResourceState}'");

            await RunComponentTaskAsync()
                .ConfigureAwait(true);

            await commandQueue
                .AddAsync(new ComponentUpdateCommand(command.User, component))
                .ConfigureAwait(true);
        }

        async Task RunComponentTaskAsync()
        {
            commandResult.Result = await orchestrationContext
                .CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskRunnerActivity), new ComponentTaskRunnerActivity.Input() { ComponentTask = commandResult.Result })
                .ConfigureAwait(true);

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

            if (commandResult.Result.TaskState == TaskState.Failed)
            {
                throw new Exception($"Component task '{commandResult.Result.Id}' exit with code {commandResult.Result.ExitCode?.ToString() ?? "UNKNOWN"}");
            }
        }
    }

    private Task<Component> UpdateComponentAsync(IDurableOrchestrationContext orchestrationContext, Component component, ResourceState? resourceState = null)
    {
        component.ResourceState = resourceState.GetValueOrDefault(component.ResourceState);

        return orchestrationContext.CallActivityWithRetryAsync<Component>(nameof(ComponentSetActivity), new ComponentSetActivity.Input() { Component = component });
    }

    private Task<ComponentTask> UpdateComponentTaskAsync(IDurableOrchestrationContext orchestrationContext, ComponentTask componentTask, TaskState? taskState = null)
    {
        componentTask.TaskState = taskState.GetValueOrDefault(componentTask.TaskState);

        return orchestrationContext.CallActivityWithRetryAsync<ComponentTask>(nameof(ComponentTaskSetActivity), new ComponentTaskSetActivity.Input() { ComponentTask = componentTask });
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
