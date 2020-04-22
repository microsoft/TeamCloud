/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TeamCloud.Http;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator
{
    public static class CommandTrigger
    {
        [FunctionName(nameof(CommandTrigger))]
        public static async Task<IActionResult> RunTrigger(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "command")] HttpRequestMessage httpRequest,
            [DurableClient] IDurableClient durableClient)
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var orchestratorCommand = await httpRequest.Content
                .ReadAsJsonAsync<IOrchestratorCommand>()
                .ConfigureAwait(false);

            var commandResult = await StartCommandOrchestration(durableClient, orchestratorCommand)
                .ConfigureAwait(false);

            return new OkObjectResult(commandResult);
        }

        private static async Task<ICommandResult> StartCommandOrchestration(IDurableClient durableClient, IOrchestratorCommand orchestratorCommand)
        {
            var orchestratorCommandMessage = new OrchestratorCommandMessage(orchestratorCommand);
            var orchestratorCommandResult = orchestratorCommand.CreateResult();

            var orchestratorCommandOrchestration = orchestratorCommand switch
            {
                _ => $"{orchestratorCommand.GetType().Name}Orchestration"
            };

            try
            {
                var instanceId = await durableClient
                    .StartNewAsync<object>(orchestratorCommandOrchestration, orchestratorCommand.CommandId.ToString(), orchestratorCommandMessage)
                    .ConfigureAwait(false);

                var status = await durableClient
                    .GetStatusAsync(instanceId)
                    .ConfigureAwait(false);

                orchestratorCommandResult.ApplyStatus(status);
            }
            catch (FunctionFailedException exc)
            {
                orchestratorCommandResult.Errors.Add(exc);
            }

            return orchestratorCommandResult;
        }
    }
}
