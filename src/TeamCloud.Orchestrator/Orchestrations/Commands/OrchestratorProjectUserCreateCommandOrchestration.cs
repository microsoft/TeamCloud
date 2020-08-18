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
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Internal;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Entities;
using TeamCloud.Orchestrator.Orchestrations.Utilities;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class OrchestratorProjectUserCreateCommandOrchestration
    {
        [FunctionName(nameof(OrchestratorProjectUserCreateCommandOrchestration))]
        public static async Task RunOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var command = functionContext.GetInput<OrchestratorProjectUserCreateCommand>();
            var commandResult = command.CreateResult();
            var user = command.Payload;

            using (log.BeginCommandScope(command))
            {
                try
                {
                    functionContext.SetCustomStatus($"Creating user", log);

                    using (await functionContext.LockContainerDocumentAsync(user).ConfigureAwait(true))
                    {
                        user = await functionContext
                            .SetUserProjectMembershipAsync(user, command.ProjectId)
                            .ConfigureAwait(true);
                    }

                    functionContext.SetCustomStatus("Sending commands", log);

                    var providerCommand = new ProviderProjectUserCreateCommand
                    (
                        command.User.PopulateExternalModel(),
                        user.PopulateExternalModel(),
                        command.ProjectId,
                        command.CommandId
                    );

                    var providerResults = await functionContext
                        .SendProviderCommandAsync(providerCommand, null)
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

                    commandResult.Result = user;

                    functionContext.SetOutput(commandResult);
                }
            }
        }
    }
}
