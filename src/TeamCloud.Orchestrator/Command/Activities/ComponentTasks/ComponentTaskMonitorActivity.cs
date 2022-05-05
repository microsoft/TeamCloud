/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.ComponentTasks;

public sealed class ComponentDeploymentMonitorActivity
{
    private readonly IComponentTaskRepository componentTaskRepository;
    private readonly IProjectRepository projectRepository;
    private readonly IAzureService azure;

    public ComponentDeploymentMonitorActivity(IComponentTaskRepository componentTaskRepository, IProjectRepository projectRepository, IAzureService azure)
    {
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.azure = azure ?? throw new ArgumentNullException(nameof(azure));
    }

    [FunctionName(nameof(ComponentDeploymentMonitorActivity))]
    [RetryOptions(3)]
    public async Task<ComponentTask> Run(
        [ActivityTrigger] IDurableActivityContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var componentTask = context.GetInput<Input>().ComponentTask;

        try
        {

            if (!string.IsNullOrEmpty(componentTask.ResourceId) && await azure.ExistsAsync(componentTask.ResourceId).ConfigureAwait(false))
            {
                var group = await azure.ContainerInstances
                    .GetGroupAsync(componentTask.ResourceId)
                    .ConfigureAwait(false);

                var runner = group.Containers
                    .SingleOrDefault(c => c.Name.Equals(componentTask.Id, StringComparison.OrdinalIgnoreCase));

                if (runner?.InstanceView is null)
                {
                    componentTask.TaskState = TaskState.Initializing;
                }
                else if (runner.InstanceView.CurrentState is not null)
                {
                    componentTask.TaskState = TaskState.Processing;
                    componentTask.ExitCode = runner.InstanceView.CurrentState.ExitCode;
                    componentTask.Started = runner.InstanceView.CurrentState.StartTime;
                    componentTask.Finished = runner.InstanceView.CurrentState.FinishTime;

                    if (componentTask.ExitCode.HasValue)
                    {
                        componentTask.TaskState = componentTask.ExitCode == 0
                            ? TaskState.Succeeded   // ExitCode indicates successful provisioning
                            : TaskState.Failed;     // ExitCode indicates failed provisioning
                    }
                    else if (runner.InstanceView.CurrentState.State?.Equals("Terminated", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // container instance was terminated without exit code
                        componentTask.TaskState = TaskState.Failed;
                    }

                    if (componentTask.TaskState == TaskState.Failed && !componentTask.ExitCode.HasValue)
                    {
                        var output = new StringBuilder();

                        output.AppendLine($"Creating runner {group.Id} ended in state {group.ProvisioningState} !!! {Environment.NewLine}");
                        output.AppendLine(await azure.ContainerInstances.GetLogsAsync(group.Id, runner.Name).ConfigureAwait(false));

                        var project = await projectRepository
                            .GetAsync(componentTask.Organization, componentTask.ProjectId)
                            .ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(project?.StorageId))
                        {
                            if (await azure.ExistsAsync(project.StorageId).ConfigureAwait(false))
                            {
                                var outputDirectory = ".output";

                                var fileClient = await azure.Storage.FileShares
                                    .GetShareFileClientAsync(project.StorageId, componentTask.ComponentId, outputDirectory, componentTask.Id, ensureDirectroyExists: true)
                                    .ConfigureAwait(false);

                                if (!await fileClient.ExistsAsync().ConfigureAwait(false))
                                {
                                    var logBuffer = Encoding.Default.GetBytes(output.ToString());

                                    using var fileStream = await fileClient
                                        .OpenWriteAsync(true, 0, options: new ShareFileOpenWriteOptions() { MaxSize = logBuffer.Length })
                                        .ConfigureAwait(false);

                                    await fileStream
                                        .WriteAsync(logBuffer)
                                        // .WriteAsync(logBuffer, 0, logBuffer.Length)
                                        .ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }

                componentTask = await componentTaskRepository
                    .SetAsync(componentTask)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception exc)
        {
            throw exc.AsSerializable();
        }

        return componentTask;
    }

    internal struct Input
    {
        public ComponentTask ComponentTask { get; set; }
    }
}
