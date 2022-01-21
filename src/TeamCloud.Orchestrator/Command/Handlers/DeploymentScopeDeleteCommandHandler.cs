/**
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

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class DeploymentScopeDeleteCommandHandler : CommandHandler,
    ICommandHandler<DeploymentScopeDeleteCommand>
{
    private readonly IDeploymentScopeRepository deploymentScopeRepository;

    public DeploymentScopeDeleteCommandHandler(IDeploymentScopeRepository deploymentScopeRepository)
    {
        this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
    }

    public override bool Orchestration => false;

    public async Task<ICommandResult> HandleAsync(DeploymentScopeDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await deploymentScopeRepository
                .RemoveAsync(command.Payload)
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
