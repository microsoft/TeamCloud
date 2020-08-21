using System;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Handlers
{
    public sealed class OrchestratorProjectLinkCommandHandler
        : IOrchestratorCommandHandler<OrchestratorProjectLinkCreateCommand>,
          IOrchestratorCommandHandler<OrchestratorProjectLinkUpdateCommand>,
          IOrchestratorCommandHandler<OrchestratorProjectLinkDeleteCommand>
    {
        private readonly IProjectLinkRepository projectLinkRepository;

        public OrchestratorProjectLinkCommandHandler(IProjectLinkRepository projectLinkRepository)
        {
            this.projectLinkRepository = projectLinkRepository ?? throw new ArgumentNullException(nameof(projectLinkRepository));
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorProjectLinkCreateCommand orchestratorCommand)
        {
            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await projectLinkRepository
                    .AddAsync(orchestratorCommand.Payload)
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(OrchestratorProjectLinkUpdateCommand orchestratorCommand)
        {
            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await projectLinkRepository
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

        public async Task<ICommandResult> HandleAsync(OrchestratorProjectLinkDeleteCommand orchestratorCommand)
        {
            var commandResult = orchestratorCommand.CreateResult();

            try
            {
                commandResult.Result = await projectLinkRepository
                    .RemoveAsync(orchestratorCommand.Payload)
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
