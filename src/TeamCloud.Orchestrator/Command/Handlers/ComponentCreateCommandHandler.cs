﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ComponentCreateCommandHandler : CommandHandler<ComponentCreateCommand>
    {
        private readonly IComponentRepository componentRepository;

        public ComponentCreateCommandHandler(IComponentRepository componentRepository)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }

        public override async Task<ICommandResult> HandleAsync(ComponentCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await componentRepository
                    .AddAsync(command.Payload)
                    .ConfigureAwait(false);

                var componentTask = new ComponentTask
                {
                    Organization = commandResult.Result.Organization,
                    ComponentId = commandResult.Result.Id,
                    ProjectId = commandResult.Result.ProjectId,
                    Type = ComponentTaskType.Create,
                    RequestedBy = commandResult.Result.Creator,
                    InputJson = commandResult.Result.InputJson
                };

                await commandQueue
                    .AddAsync(new ComponentTaskCreateCommand(command.User, componentTask))
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }
    }
}
