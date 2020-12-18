/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Operations.Activities;
using TeamCloud.Orchestrator.Operations.Activities.Templates;
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
                        .CallActivityWithRetryAsync<Organization>(nameof(OrganizationGetActivity), new OrganizationGetActivity.Input() { Id = command.Payload.Id })
                        .ConfigureAwait(true)) ?? command.Payload;

                    try
                    {
                        organization = await context
                            .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = organization, ResourceState = ResourceState.Initializing })
                            .ConfigureAwait(true);

                        organization = await context
                            .CallActivityWithRetryAsync<Organization>(nameof(OrganizationInitActivity), new OrganizationInitActivity.Input() { Organization = organization })
                            .ConfigureAwait(true);

                        var deploymentOutputEventName = context.NewGuid().ToString();

                        _ = await context
                            .StartDeploymentAsync(nameof(OrganizationDeployActivity), new OrganizationDeployActivity.Input() { Organization = organization }, deploymentOutputEventName)
                            .ConfigureAwait(true);

                        organization = await context
                            .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = organization, ResourceState = ResourceState.Provisioning })
                            .ConfigureAwait(true);

                        _ = await context
                            .WaitForDeploymentOutput(deploymentOutputEventName, TimeSpan.FromMinutes(5))
                            .ConfigureAwait(true);

                        organization = await context
                            .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = organization, ResourceState = ResourceState.Succeeded })
                            .ConfigureAwait(true);
                    }
                    catch (Exception deploymentExc)
                    {
                        log.LogError(deploymentExc, $"Failed to deploy resources for organization {organization.Id}: {deploymentExc.Message}");

                        organization = await context
                            .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = organization, ResourceState = ResourceState.Failed })
                            .ConfigureAwait(true);
                    }
                    finally
                    {
                        commandResult.Result = organization;
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
