using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentDeploymentMonitorActivity
    {
        private readonly IComponentDeploymentRepository componentDeploymentRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentDeploymentMonitorActivity(IComponentDeploymentRepository componentDeploymentRepository, IAzureResourceService azureResourceService)
        {
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentDeploymentMonitorActivity))]
        [RetryOptions(3)]
        public async Task<ComponentDeployment> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var componentDeployment = context.GetInput<Input>().ComponentDeployment;

            try
            {

                if (AzureResourceIdentifier.TryParse(componentDeployment.ResourceId, out var resourceId)
                    && await azureResourceService.ExistsResourceAsync(resourceId.ToString()).ConfigureAwait(false))
                {
                    var session = await azureResourceService.AzureSessionService
                        .CreateSessionAsync(resourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    var runner = await session.ContainerGroups
                        .GetByIdAsync(resourceId.ToString())
                        .ConfigureAwait(false);

                    var container = runner.Containers
                        .SingleOrDefault()
                        .Value;

                    if (container?.InstanceView is null)
                    {
                        componentDeployment.ResourceState = ResourceState.Initializing;
                    }
                    else if (container.InstanceView.CurrentState != null)
                    {
                        componentDeployment.ResourceState = ResourceState.Provisioning;
                        componentDeployment.ExitCode = container.InstanceView.CurrentState.ExitCode;
                        componentDeployment.Started = container.InstanceView.CurrentState.StartTime;
                        componentDeployment.Finished = container.InstanceView.CurrentState.FinishTime;

                        if (componentDeployment.ExitCode.HasValue)
                        {
                            componentDeployment.ResourceState = componentDeployment.ExitCode == 0
                                ? ResourceState.Succeeded   // ExitCode indicates successful provisioning
                                : ResourceState.Failed;     // ExitCode indicates failed provisioning
                        }
                        else if (container.InstanceView.CurrentState.State?.Equals("Terminated", StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            // container instance was terminated without exit code
                            componentDeployment.ResourceState = ResourceState.Failed;
                        }
                    }

                    componentDeployment = await componentDeploymentRepository
                        .SetAsync(componentDeployment)
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
