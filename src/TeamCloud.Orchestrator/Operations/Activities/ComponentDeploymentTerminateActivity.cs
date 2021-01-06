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
    public sealed class ComponentDeploymentTerminateActivity
    {
        private readonly IComponentDeploymentRepository componentDeploymentRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentDeploymentTerminateActivity(IComponentDeploymentRepository componentDeploymentRepository, IAzureResourceService azureResourceService)
        {
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentDeploymentTerminateActivity))]
        [RetryOptions(3)]
        public async Task<ComponentDeployment> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var componentDeployment = context.GetInput<Input>().ComponentDeployment;

            try
            {
                if (AzureResourceIdentifier.TryParse(componentDeployment.ResourceId, out var resourceId)
                    && await azureResourceService.ExistsResourceAsync(resourceId.ToString()).ConfigureAwait(false))
                {
                    if (!componentDeployment.ResourceState.IsFinal())
                    {
                        componentDeployment.ResourceState = ResourceState.Failed;

                        componentDeployment = await componentDeploymentRepository
                            .SetAsync(componentDeployment)
                            .ConfigureAwait(false);
                    }

                    var session = await azureResourceService.AzureSessionService
                        .CreateSessionAsync(resourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    await session.ContainerGroups
                        .DeleteByIdAsync(resourceId.ToString())
                        .ConfigureAwait(false);
                }
            }
            catch (Exception exc)
            {
                throw exc.AsSerializable();
            }

            return componentDeployment;
        }

        internal struct Input
        {
            public ComponentDeployment ComponentDeployment { get; set; }
        }
    }
}
