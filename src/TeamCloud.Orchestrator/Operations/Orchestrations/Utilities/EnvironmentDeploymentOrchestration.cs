using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Utilities
{
    public static class EnvironmentDeploymentOrchestration
    {
        [FunctionName(nameof(EnvironmentDeploymentOrchestration))]
        public static async Task<Component> Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var input = context.GetInput<Input>();
            var component = input.Component;
            var componentDeployment = input.ComponentDeployment;

            try
            {
                if (component.Type != ComponentType.Environment)
                    throw new NotSupportedException($"Components of type '{component.Type}' are not supported");

                using (await context.LockContainerDocumentAsync(component).ConfigureAwait(true))
                {
                    if (componentDeployment is null)
                    {
                        component = await UpdateComponentAsync(component, ResourceState.Initializing)
                            .ConfigureAwait(true);

                        component = await context
                            .CallActivityWithRetryAsync<Component>(nameof(ComponentInitActivity), new ComponentInitActivity.Input() { Component = component })
                            .ConfigureAwait(true);

                        component = await UpdateComponentAsync(component)
                            .ConfigureAwait(true);

                        componentDeployment = await context
                            .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentRunnerActivity), new ComponentDeploymentRunnerActivity.Input() { Component = component })
                            .ConfigureAwait(true);

                        context.ContinueAsNew(new Input() { Component = component, ComponentDeployment = componentDeployment });
                    }
                    else
                    {
                        componentDeployment = await context
                            .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentUpdateActivity), new ComponentDeploymentUpdateActivity.Input() { ComponentDeployment = componentDeployment })
                            .ConfigureAwait(true);

                        if (componentDeployment.ResourceState == ResourceState.Succeeded || componentDeployment.ResourceState == ResourceState.Failed)
                        {
                            component = await context
                                .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ProjectId = component.ProjectId, ComponentId = component.Id })
                                .ConfigureAwait(true);
                        }
                        else
                        {
                            // deployment is still in progress - restart the orchestration and keep monitoring
                            context.ContinueAsNew(new Input() { Component = component, ComponentDeployment = componentDeployment });
                        }

                    }
                }

                return component;
            }
            catch (Exception exc)
            {
                _ = await UpdateComponentAsync(component, ResourceState.Failed)
                    .ConfigureAwait(true);

                throw exc.AsSerializable();
            }

            Task<Component> UpdateComponentAsync(Component component, ResourceState? resourceState = null)
            {
                component.ResourceState = resourceState.GetValueOrDefault(component.ResourceState);

                return context.CallActivityWithRetryAsync<Component>(nameof(ComponentSetActivity), new ComponentSetActivity.Input() { Component = component });
            }
        }

        internal struct Input
        {
            public Component Component { get; set; }

            public ComponentDeployment ComponentDeployment { get; set; }
        }

    }
}
