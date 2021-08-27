/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Command.Activities.Organizations;

using TeamCloud.Orchestrator.Command.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class OrganizationDeployCommandHandler : CommandHandler<OrganizationDeployCommand>
    {
        public OrganizationDeployCommandHandler() : base(true)
        { }

        public override async Task<ICommandResult> HandleAsync(OrganizationDeployCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var commandResult = command.CreateResult();

            using (await orchestrationContext.LockContainerDocumentAsync(command.Payload).ConfigureAwait(true))
            {
                // just to make sure we are dealing with the latest version
                // of the organization entity, we re-fetch the entity and
                // use the passed in one as a potential fallback.

                commandResult.Result = (await orchestrationContext
                    .CallActivityWithRetryAsync<Organization>(nameof(OrganizationGetActivity), new OrganizationGetActivity.Input() { Id = command.Payload.Id })
                    .ConfigureAwait(true)) ?? command.Payload;

                try
                {
                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = commandResult.Result, ResourceState = ResourceState.Initializing })
                        .ConfigureAwait(true);

                    var deploymentOutputEventName = orchestrationContext.NewGuid().ToString();

                    _ = await orchestrationContext
                        .StartDeploymentAsync(nameof(OrganizationDeployActivity), new OrganizationDeployActivity.Input() { Organization = commandResult.Result }, deploymentOutputEventName)
                        .ConfigureAwait(true);

                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = commandResult.Result, ResourceState = ResourceState.Provisioning })
                        .ConfigureAwait(true);

                    var deploymentOutput = await orchestrationContext
                        .WaitForDeploymentOutput(deploymentOutputEventName, TimeSpan.FromMinutes(5))
                        .ConfigureAwait(true);

                    if (deploymentOutput.TryGetValue("organizationData", out var organizationData) && organizationData is JObject organizationDataJson)
                    {
                        commandResult.Result = TeamCloudSerialize.MergeObject(organizationDataJson.ToString(), commandResult.Result);

                        commandResult.Result = await orchestrationContext
                            .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = commandResult.Result, ResourceState = ResourceState.Provisioned })
                            .ConfigureAwait(true);
                    }
                    else
                    {
                        throw new NullReferenceException($"Deployment output doesn't contain 'organizationData' output.");
                    }

                }
                catch
                {
                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = commandResult.Result, ResourceState = ResourceState.Failed })
                        .ConfigureAwait(true);

                    throw;
                }
            }

            return commandResult;
        }
    }
}