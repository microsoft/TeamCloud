/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.ComponentTasks;

public sealed class ComponentTaskTerminateActivity
{
    private readonly IComponentTaskRepository componentTaskRepository;
    private readonly IAzureService azure;

    public ComponentTaskTerminateActivity(IComponentTaskRepository componentTaskRepository, IAzureService azure)
    {
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        this.azure = azure ?? throw new ArgumentNullException(nameof(azure));
    }

    [FunctionName(nameof(ComponentTaskTerminateActivity))]
    [RetryOptions(3)]
    public async Task<ComponentTask> Run(
        [ActivityTrigger] IDurableActivityContext context,
        ILogger log)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        if (log is null)
            throw new ArgumentNullException(nameof(log));

        var componentTask = context.GetInput<Input>().ComponentTask;

        try
        {
            if (!string.IsNullOrEmpty(componentTask.ResourceId))
            {
                if (!componentTask.TaskState.IsFinal())
                {
                    componentTask.TaskState = TaskState.Failed;

                    componentTask = await componentTaskRepository
                        .SetAsync(componentTask)
                        .ConfigureAwait(false);
                }

                var containerGroup = await azure.ContainerInstances
                    .GetGroupAsync(componentTask.ResourceId)
                    .ConfigureAwait(false);

                if (containerGroup is not null)
                {
                    try
                    {
                        await azure.ContainerInstances
                            .StopAsync(containerGroup.Id)
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        // swallow
                    }
                    finally
                    {
                        var location = containerGroup.Location;

                        var id = new ResourceIdentifier(containerGroup.Id);

                        var usageData = await azure.ContainerInstances
                            .GetUsageAsync(id.SubscriptionId, location)
                            .ConfigureAwait(false);

                        var usage = usageData
                            .SingleOrDefault(u => u.Unit.Equals("Count") && u.Name.Value.Equals("ContainerGroups"));

                        var limit = usage?.Limit.GetValueOrDefault() ?? 0;
                        var current = usage?.CurrentValue.GetValueOrDefault() ?? 0;

                        if (current >= limit)
                        {
                            await azure
                                .DeleteResourceAsync(containerGroup.Id)
                                .ConfigureAwait(false);
                        }
                    }
                }
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
