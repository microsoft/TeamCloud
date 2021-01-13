using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Operations.Activities;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Utilities
{
    public sealed class ComponentMonitorOrchestration
    {
        private readonly IComponentRepository componentRepository;
        private readonly IUserRepository userRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentMonitorOrchestration(IComponentRepository componentRepository, IUserRepository userRepository, IAzureResourceService azureResourceService)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentMonitorOrchestration))]
        public async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var input = context.GetInput<Input>();

            if (context.InstanceId.Equals(input.ComponentId, StringComparison.OrdinalIgnoreCase))
            {
                var component = await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ProjectId = input.ProjectId, ComponentId = input.ComponentId })
                    .ConfigureAwait(true);

                if (component != null)
                {
                    try
                    {
                        component = await context
                            .CallSubOrchestratorWithRetryAsync<Component>(nameof(ComponentPrepareOrchestration), new ComponentPrepareOrchestration.Input() { Component = component })
                            .ConfigureAwait(true);
                    }
                    catch (Exception exc)
                    {
                        log.LogWarning(exc, $"Lifetime orchestration failed for component {component.Id} ({component.Slug}) in project {component.ProjectId}: {exc.Message}");
                    }
                    finally
                    {
                        await context
                            .ContinueAsNew(input, TimeSpan.FromMinutes(5))
                            .ConfigureAwait(true);
                    }
                }
            }
            else
            {
                log.LogError($"Lifetime orchestrations must have the same identifier ({context.InstanceId}) as the monitored component ({input.ComponentId}).");
            }
        }

        internal struct Input
        {
            public string ComponentId { get; set; }

            public string ProjectId { get; set; }
        }
    }
}
