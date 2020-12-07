using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Utilities
{
    public static class EnvironmentDeployOrchestration
    {
        [FunctionName(nameof(EnvironmentDeployOrchestration))]
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
                        var project = await context
                            .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Organization = component.Organization, Id = component.ProjectId })
                            .ConfigureAwait(true);

                        var deploymentScope = await context
                            .CallActivityWithRetryAsync<DeploymentScope>(nameof(DeploymentScopeGetActivity), new DeploymentScopeGetActivity.Input() { Organization = component.Organization, Id = component.DeploymentScopeId })
                            .ConfigureAwait(true);

                        if (!AzureResourceIdentifier.TryParse(component.IdentityId, out var _))
                        {
                            var deploymentScopeInitAcitivityEvent = context.NewGuid().ToString();

                            _ = await context
                                .StartDeploymentAsync(nameof(DeploymentScopeInitAcitivity), new DeploymentScopeInitAcitivity.Input() { Project = project, DeploymentScope = deploymentScope }, deploymentScopeInitAcitivityEvent)
                                .ConfigureAwait(true);

                            component = await UpdateComponentAsync(component, ResourceState.Initializing)
                                .ConfigureAwait(true);

                            var deploymentScopeInitAcitivityOutput = await context
                                .WaitForDeploymentOutput(deploymentScopeInitAcitivityEvent, TimeSpan.FromMinutes(5))
                                .ConfigureAwait(true);

                            component.IdentityId = deploymentScopeInitAcitivityOutput["identityId"].ToString();

                            component = await UpdateComponentAsync(component)
                                .ConfigureAwait(true);
                        }

                        if (!AzureResourceIdentifier.TryParse(component.ResourceId, out var _))
                        {
                            var environmentInitActivityEvent = context.NewGuid().ToString();

                            _ = await context
                                .StartDeploymentAsync(nameof(EnvironmentInitActivity), new EnvironmentInitActivity.Input() { DeploymentScope = deploymentScope, Component = component }, environmentInitActivityEvent)
                                .ConfigureAwait(true);

                            component = await UpdateComponentAsync(component, ResourceState.Initializing)
                                .ConfigureAwait(true);

                            var environmentInitActivityOutput = await context
                                .WaitForDeploymentOutput(environmentInitActivityEvent, TimeSpan.FromMinutes(5))
                                .ConfigureAwait(true);

                            component.ResourceId = environmentInitActivityOutput["resourceId"].ToString();

                            component = await UpdateComponentAsync(component)
                                .ConfigureAwait(true);
                        }

                        var deploymentOutputEventName = context.NewGuid().ToString();

                        _ = await context
                            .StartDeploymentAsync(nameof(EnvironmentDeployActivity), new EnvironmentDeployActivity.Input() { Component = component }, deploymentOutputEventName)
                            .ConfigureAwait(true);

                        component = await UpdateComponentAsync(component, ResourceState.Initializing)
                            .ConfigureAwait(true);

                        var deploymentOutput = await context
                            .WaitForDeploymentOutput(deploymentOutputEventName, TimeSpan.FromMinutes(5))
                            .ConfigureAwait(true);

                        componentDeployment = new ComponentDeployment()
                        {
                            ComponentId = component.Id,
                            ProjectId = component.ProjectId,
                            ResourceId = deploymentOutput["runnerId"].ToString()
                        };

                        componentDeployment = await context
                            .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentSetActivity), new ComponentDeploymentSetActivity.Input() { ComponentDeployment = componentDeployment })
                            .ConfigureAwait(true);

                        context.ContinueAsNew(new Input() { Component = component, ComponentDeployment = componentDeployment });
                    }
                    else
                    {
                        componentDeployment = await context
                            .CallActivityWithRetryAsync<ComponentDeployment>(nameof(ComponentDeploymentRefreshActivity), new ComponentDeploymentRefreshActivity.Input() { ComponentDeployment = componentDeployment })
                            .ConfigureAwait(true);

                        if (componentDeployment.ExitCode.HasValue)
                        {
                            component = await context
                                .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ProjectId = component.ProjectId, Id = component.Id })
                                .ConfigureAwait(false);
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

        public struct Input
        {
            public Component Component { get; set; }

            public ComponentDeployment ComponentDeployment { get; set; }
        }

    }
}
