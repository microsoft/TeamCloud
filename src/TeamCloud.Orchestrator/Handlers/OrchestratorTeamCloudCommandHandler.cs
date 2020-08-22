/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Handlers
{
    public sealed class OrchestratorTeamCloudCommandHandler
        : IOrchestratorCommandHandler<OrchestratorTeamCloudInstanceSetCommand>
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public OrchestratorTeamCloudCommandHandler(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorTeamCloudInstanceSetCommand orchestratorCommand)
        {
            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await teamCloudRepository
                    .SetAsync(orchestratorCommand.Payload)
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
