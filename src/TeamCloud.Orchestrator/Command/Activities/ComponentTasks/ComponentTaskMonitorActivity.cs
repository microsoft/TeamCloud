/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

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

namespace TeamCloud.Orchestrator.Command.Activities.ComponentTasks
{
    public sealed class ComponentDeploymentMonitorActivity
    {
        private readonly IComponentTaskRepository componentTaskRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentDeploymentMonitorActivity(IComponentTaskRepository componentTaskRepository, IAzureResourceService azureResourceService)
        {
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentDeploymentMonitorActivity))]
        [RetryOptions(3)]
        public async Task<ComponentTask> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var componentDeployment = context.GetInput<Input>().ComponentTask;

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

                        if (componentDeployment.ResourceState == ResourceState.Failed)
                        {
                            var log = await runner
                                .GetLogContentAsync(container.Name)
                                .ConfigureAwait(false);
                        }
                    }

                    componentDeployment = await componentTaskRepository
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
            public ComponentTask ComponentTask { get; set; }
        }
    }
}
