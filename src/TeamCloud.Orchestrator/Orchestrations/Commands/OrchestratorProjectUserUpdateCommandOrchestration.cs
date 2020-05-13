/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProjectUserUpdateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectUserUpdateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorProjectUserUpdateCommand>();
            var commandResult = command.CreateResult();
            var commandProject = default(Project);

            using (log.BeginCommandScope(command))
            {
                try
                {
                    functionContext.SetCustomStatus($"Deleting user", log);

                    using (await functionContext.LockAsync<Project>(command.ProjectId.ToString()).ConfigureAwait(true))
                    {
                        commandProject = await functionContext
                            .GetProjectAsync(command.ProjectId.GetValueOrDefault())
                            .ConfigureAwait(true);

                        if (commandProject.Users.Remove(command.Payload))
                        {
                            commandProject = await functionContext
                                .SetProjectAsync(commandProject)
                                .ConfigureAwait(true);
                        }
                    }

                    functionContext.SetCustomStatus("Sending commands", log);

                    var providerCommand = new ProviderProjectUserUpdateCommand
                    (
                        command.User,
                        command.Payload,
                        commandProject.Id,
                        command.CommandId
                    );

                    var providerResults = await functionContext
                        .SendCommandAsync<ProviderProjectUserUpdateCommand>(providerCommand, commandProject)
                        .ConfigureAwait(true);

                    var providerException = providerResults.Values?
                        .SelectMany(result => result.Errors ?? new List<CommandError>())
                        .ToException();

                    if (providerException != null)
                        throw providerException;
                }
                catch (Exception exc)
                {
                    commandResult ??= command.CreateResult();
                    commandResult.Errors.Add(exc);

                    throw;
                }
                finally
                {
                    var commandException = commandResult.Errors?.ToException();

                    if (commandException is null)
                        functionContext.SetCustomStatus($"Command succeeded", log);
                    else
                        functionContext.SetCustomStatus($"Command failed", log, commandException);

                    commandResult.Result = command.Payload;

                    functionContext.SetOutput(commandResult);
                }
            }
        }
    }
}
