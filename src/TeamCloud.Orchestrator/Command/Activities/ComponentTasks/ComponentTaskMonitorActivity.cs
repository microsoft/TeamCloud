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
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
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
    private readonly IAzureResourceService azureResourceService;

    public ComponentDeploymentMonitorActivity(IComponentTaskRepository componentTaskRepository, IProjectRepository projectRepository, IAzureResourceService azureResourceService)
    {
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
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

            if (AzureResourceIdentifier.TryParse(componentTask.ResourceId, out var resourceId)
                && await azureResourceService.ExistsResourceAsync(resourceId.ToString()).ConfigureAwait(false))
            {
                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync(resourceId.SubscriptionId)
                    .ConfigureAwait(false);

                var group = await session.ContainerGroups
                    .GetByIdAsync(resourceId.ToString())
                    .ConfigureAwait(false);

                var runner = group.Containers
                    .SingleOrDefault(c => c.Key.Equals(componentTask.Id, StringComparison.OrdinalIgnoreCase))
                    .Value;

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

                        output.AppendLine($"Creating runner {group.Id} ended in state {group.State} !!! {Environment.NewLine}");
                        output.AppendLine(await group.GetLogContentAsync(runner.Name).ConfigureAwait(false));

                        var project = await projectRepository
                            .GetAsync(componentTask.Organization, componentTask.ProjectId)
                            .ConfigureAwait(false);

                        if (AzureResourceIdentifier.TryParse(project?.StorageId, out var storageId))
                        {
                            var storage = await azureResourceService
                                .GetResourceAsync<AzureStorageAccountResource>(storageId.ToString())
                                .ConfigureAwait(false);

                            if (storage is not null)
                            {
                                var outputDirectory = ".output";

                                var fileClient = await storage
                                    .CreateShareFileClientAsync(componentTask.ComponentId, $"{outputDirectory}/{componentTask.Id}")
                                    .ConfigureAwait(false);

                                if (!await fileClient.ExistsAsync().ConfigureAwait(false))
                                {
                                    await storage
                                        .EnsureDirectoryPathAsync(componentTask.ComponentId, outputDirectory)
                                        .ConfigureAwait(false);

                                    var logBuffer = Encoding.Default.GetBytes(output.ToString());

                                    using var fileStream = await fileClient
                                        .OpenWriteAsync(true, 0, options: new ShareFileOpenWriteOptions() { MaxSize = logBuffer.Length })
                                        .ConfigureAwait(false);

                                    await fileStream
                                        .WriteAsync(logBuffer, 0, logBuffer.Length)
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
