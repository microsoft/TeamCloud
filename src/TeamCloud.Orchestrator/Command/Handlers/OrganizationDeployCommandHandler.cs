/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Model.Messaging;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator.Command.Activities.Organizations;
using TeamCloud.Orchestrator.Command.Activities.Portal;
using TeamCloud.Orchestrator.Command.Entities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class OrganizationDeployCommandHandler : CommandHandler<OrganizationDeployCommand>
{
    public override bool Orchestration => true;

    public override async Task<ICommandResult> HandleAsync(OrganizationDeployCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
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
                    .WaitForDeploymentOutput(deploymentOutputEventName, TimeSpan.FromMinutes(15))
                    .ConfigureAwait(true);

                if (deploymentOutput.TryGetValue("organizationData", out var organizationData) && organizationData is JObject organizationDataJson)
                {
                    commandResult.Result = TeamCloudSerialize.MergeObject(organizationDataJson.ToString(), commandResult.Result);

                    commandResult.Result = await orchestrationContext
                        .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = commandResult.Result, ResourceState = ResourceState.Provisioned })
                        .ConfigureAwait(true);

                    if (commandResult.Result.Portal != PortalType.TeamCloud)
                    {
                        bool requestConsent = await orchestrationContext
                            .CallActivityWithRetryAsync<bool>(nameof(PortalGrantPermissonsActivity), new PortalGrantPermissonsActivity.Input() { Organization = commandResult.Result })
                            .ConfigureAwait(true);

                        if (requestConsent)
                        {
                            var message = NotificationMessage.Create<PortalPermissionGrantMessage>(command.User);

                            message.Merge(new PortalPermissionGrantMessageData()
                            {
                                Organization = commandResult.Result
                            });

                            await commandQueue
                                .AddAsync(new NotificationSendMailCommand<PortalPermissionGrantMessage>(command.User, message))
                                .ConfigureAwait(true);
                        }

                        if (!string.IsNullOrEmpty(commandResult.Result.PortalReplyUrl))
                        {
                            await orchestrationContext
                                .CallActivityWithRetryAsync(nameof(PortalRegisterReplyUrlActivity), new PortalRegisterReplyUrlActivity.Input() { Organization = commandResult.Result, ReplyUrl = commandResult.Result.PortalReplyUrl })
                                .ConfigureAwait(true);
                        }
                    }
                }
                else
                {
                    throw new NullReferenceException($"Deployment output doesn't contain 'organizationData' output.");
                }

            }
            catch (Exception exc)
            {
                commandResult.Result = await orchestrationContext
                    .CallActivityWithRetryAsync<Organization>(nameof(OrganizationSetActivity), new OrganizationSetActivity.Input() { Organization = commandResult.Result, ResourceState = ResourceState.Failed })
                    .ConfigureAwait(true);

                throw exc.AsSerializable();
            }
        }

        return commandResult;
    }
}
