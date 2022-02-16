/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.ComponentTasks;

public sealed class ComponentTaskTerminateActivity
{
    private readonly IComponentTaskRepository componentTaskRepository;
    private readonly IAzureResourceService azureResourceService;

    public ComponentTaskTerminateActivity(IComponentTaskRepository componentTaskRepository, IAzureResourceService azureResourceService)
    {
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
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
            if (AzureResourceIdentifier.TryParse(componentTask.ResourceId, out var resourceId))
            {
                if (!componentTask.TaskState.IsFinal())
                {
                    componentTask.TaskState = TaskState.Failed;

                    componentTask = await componentTaskRepository
                        .SetAsync(componentTask)
                        .ConfigureAwait(false);
                }

                var containerGroup = await azureResourceService
                    .GetResourceAsync<AzureContainerGroupResource>(resourceId.ToString())
                    .ConfigureAwait(false);

                if (containerGroup is not null)
                {
                    try
                    {
                        await containerGroup
                            .StopAsync()
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        // swallow
                    }
                    finally
                    {
                        var location = await containerGroup
                            .GetLocationAsync()
                            .ConfigureAwait(false);

                        var usageData = await AzureContainerGroupResource
                            .GetUsageAsync(azureResourceService, containerGroup.ResourceId.SubscriptionId, location)
                            .ConfigureAwait(false);

                        var usage = usageData
                            .SingleOrDefault(u => u.Unit.Equals("Count") && u.Name.Value.Equals("ContainerGroups"));

                        var limit = usage?.Limit.GetValueOrDefault() ?? 0;
                        var current = usage?.CurrentValue.GetValueOrDefault() ?? 0;

                        if (current >= limit)
                        {
                            await containerGroup
                                .DeleteAsync()
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
