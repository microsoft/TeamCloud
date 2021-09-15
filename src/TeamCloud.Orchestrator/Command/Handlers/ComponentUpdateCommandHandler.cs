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
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Command.Activities.Adapters;
using TeamCloud.Orchestrator.Command.Activities.Components;
using TeamCloud.Orchestrator.Command.Entities;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ComponentUpdateCommandHandler : CommandHandler<ComponentUpdateCommand>
    {
        public override bool Orchestration => true;

        public override async Task<ICommandResult> HandleAsync(ComponentUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var commandResult = command.CreateResult();

            commandResult.Result = await orchestrationContext
                .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ProjectId = command.Payload.ProjectId, ComponentId = command.Payload.Id })
                .ConfigureAwait(true);

            using (await orchestrationContext.LockContainerDocumentAsync(commandResult.Result).ConfigureAwait(true))
            {
                commandResult.Result = await orchestrationContext
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentGetActivity), new ComponentGetActivity.Input() { ProjectId = command.Payload.ProjectId, ComponentId = command.Payload.Id })
                    .ConfigureAwait(true);

                commandResult.Result = await orchestrationContext
                    .CallActivityWithRetryAsync<Component>(nameof(AdapterUpdateComponentActivity), new AdapterUpdateComponentActivity.Input() { Component = commandResult.Result, User = command.User })
                    .ConfigureAwait(true);
            }

            return commandResult;
        }
    }
}
