using System;
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

            try
            {
                var input = context.GetInput<Input>();
                var component = input.Component;

                if (component.Type != ComponentType.Environment)
                    throw new NotSupportedException($"Components of type '{component.Type}' are not supported");

                using (await context.LockContainerDocumentAsync(component).ConfigureAwait(true))
                {
                    var project = await context
                        .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), new ProjectGetActivity.Input() { Organization = component.Organization, Id = component.ProjectId })
                        .ConfigureAwait(true);

                    var deploymentScope = await context
                        .CallActivityWithRetryAsync<DeploymentScope>(nameof(DeploymentScopeGetActivity), new DeploymentScopeGetActivity.Input() { Organization = component.Organization, Id = component.DeploymentScopeId })
                        .ConfigureAwait(true);

                    if (string.IsNullOrEmpty(component.IdentityId))
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
                    }

                    if (string.IsNullOrEmpty(component.ResourceId))
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
                    }

                    var deploymentOutputEventName = context.NewGuid().ToString();

                    _ = await context
                        .StartDeploymentAsync(nameof(EnvironmentDeployActivity), new EnvironmentDeployActivity.Input() { Component = component }, deploymentOutputEventName)
                        .ConfigureAwait(true);

                    component = await UpdateComponentAsync(component, ResourceState.Provisioning)
                        .ConfigureAwait(true);

                    var deploymentOutput = await context
                        .WaitForDeploymentOutput(deploymentOutputEventName, TimeSpan.FromMinutes(5))
                        .ConfigureAwait(true);
                }

                return await context
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ProjectId = input.Component.ProjectId, Id = input.Component.Id })
                    .ConfigureAwait(true);
            }
            catch (Exception exc)
            {
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
        }

    }
}
