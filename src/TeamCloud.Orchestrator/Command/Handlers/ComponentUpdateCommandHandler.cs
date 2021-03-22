/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Command;
using TeamCloud.Orchestrator.Command.Activities;
using TeamCloud.Orchestrator.Command.Activities.Components;
using TeamCloud.Orchestrator.Command.Entities;
using TeamCloud.Orchestrator.Command.Orchestrations;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ComponentUpdateCommandHandler : ICommandHandler<ComponentUpdateCommand>
    {
        public bool Orchestration => true;

        public async Task<ICommandResult> HandleAsync(ComponentUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var commandResult = command.CreateResult();

            commandResult.Result = await orchestrationContext
                .CallSubOrchestratorWithRetryAsync<Component>(nameof(ComponentPrepareOrchestration), new ComponentPrepareOrchestration.Input() { Component = command.Payload })
                .ConfigureAwait(true);

            using (await orchestrationContext.LockContainerDocumentAsync(commandResult.Result, nameof(ComponentUpdateCommandHandler)).ConfigureAwait(true))
            {
                commandResult.Result = await orchestrationContext
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentEnsurePermissionActivity), new ComponentEnsurePermissionActivity.Input() { Component = commandResult.Result })
                    .ConfigureAwait(true);

                commandResult.Result = await orchestrationContext
                    .CallActivityWithRetryAsync<Component>(nameof(ComponentEnsureTaggingActivity), new ComponentEnsureTaggingActivity.Input() { Component = commandResult.Result })
                    .ConfigureAwait(true);
            }

            return commandResult;
        }
    }
}
