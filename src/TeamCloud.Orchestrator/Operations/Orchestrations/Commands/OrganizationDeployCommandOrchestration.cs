using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Orchestrations.Commands
{
    public static class OrganizationDeployCommandOrchestration
    {
        [FunctionName(nameof(OrganizationDeployCommandOrchestration))]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<OrganizationDeployCommand>();
            var commandResult = command.CreateResult();

            try
            {
                using (await context.LockContainerDocumentAsync(command.Payload).ConfigureAwait(true))
                {
                    // just to make sure we are dealing with the latest version
                    // of the organization entity, we re-fetch the entity and
                    // use the passed in one as a potential fallback.

                    var organization = (await context
                        .CallActivityWithRetryAsync<Organization>(nameof(OrganizationGetActivity), new OrganizationGetActivity.Input() { Id = command.Payload.Id, Tenant = command.Payload.Tenant })
                        .ConfigureAwait(true)) ?? command.Payload;

                    try
                    {
                        var deploymentOutputEventName = context.NewGuid().ToString();

                        _ = await context
                            .StartDeploymentAsync(nameof(OrganizationDeployActivity), new OrganizationDeployActivity.Input() { Organization = organization }, deploymentOutputEventName)
                            .ConfigureAwait(true);

                        organization.ResourceState = Model.Data.Core.ResourceState.Provisioning;

                        organization = await context
                            .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = organization })
                            .ConfigureAwait(true);

                        var deploymentOutput = await context
                            .WaitForDeploymentOutput(deploymentOutputEventName, TimeSpan.FromMinutes(5))
                            .ConfigureAwait(true);

                        organization.ResourceId = deploymentOutput["resourceId"].ToString();
                        organization.ResourceState = Model.Data.Core.ResourceState.Succeeded;
                    }
                    catch (Exception deploymentExc)
                    {
                        log.LogError(deploymentExc, $"Failed to deploy resources for organization {organization.Id}: {deploymentExc.Message}");
                        organization.ResourceState = Model.Data.Core.ResourceState.Failed;
                    }
                    finally
                    {
                        commandResult.Result = await context
                            .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = organization })
                            .ConfigureAwait(true);
                    }
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(OrganizationDeployCommandOrchestration)} failed: {exc.Message}");

                commandResult.Errors.Add(exc);

                throw exc.AsSerializable();
            }
            finally
            {
                context.SetOutput(commandResult);
            }
        }

    }
}
