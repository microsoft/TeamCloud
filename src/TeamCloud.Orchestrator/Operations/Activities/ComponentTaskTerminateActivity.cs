using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
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
                    if (!componentTask.ResourceState.IsFinal())
                    {
                        componentTask.ResourceState = ResourceState.Failed;

                        componentTask = await componentTaskRepository
                            .SetAsync(componentTask)
                            .ConfigureAwait(false);
                    }

                    if (await azureResourceService.ExistsResourceAsync(resourceId.ToString()).ConfigureAwait(false))
                    {
                        var session = await azureResourceService.AzureSessionService
                            .CreateSessionAsync(resourceId.SubscriptionId)
                            .ConfigureAwait(false);

                        await session.ContainerGroups
                            .DeleteByIdAsync(resourceId.ToString())
                            .ConfigureAwait(false);
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
}
