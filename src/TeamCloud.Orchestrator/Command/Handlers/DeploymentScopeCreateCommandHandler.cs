/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class DeploymentScopeCreateCommandHandler : CommandHandler,
    ICommandHandler<DeploymentScopeCreateCommand>
{
    private readonly IDeploymentScopeRepository deploymentScopeRepository;
    private readonly IUserRepository userRepository;
    private readonly IAdapterProvider adapterProvider;

    public DeploymentScopeCreateCommandHandler(
        IDeploymentScopeRepository deploymentScopeRepository,
        IUserRepository userRepository,
        IAdapterProvider adapterProvider)
    {
        this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.adapterProvider = adapterProvider ?? throw new ArgumentNullException(nameof(adapterProvider));
    }

    public override bool Orchestration => false;

    public async Task<ICommandResult> HandleAsync(DeploymentScopeCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await deploymentScopeRepository
                .AddAsync(command.Payload)
                .ConfigureAwait(false);

            if (adapterProvider.GetAdapter(commandResult.Result.Type) is IAdapterIdentity adapterIdentity)
            {
                var servicePrincipal = await adapterIdentity
                    .GetServiceIdentityAsync(commandResult.Result)
                    .ConfigureAwait(false);

                var servicePrincipalUser = await userRepository
                    .GetAsync(commandResult.Result.Organization, servicePrincipal.ObjectId.ToString())
                    .ConfigureAwait(false);

                if (servicePrincipalUser is null)
                {
                    servicePrincipalUser ??= new User
                    {
                        Id = servicePrincipal.ObjectId.ToString(),
                        Role = OrganizationUserRole.Adapter,
                        UserType = Model.Data.UserType.Service,
                        Organization = commandResult.Result.Organization,
                        OrganizationName = commandResult.Result.OrganizationName
                    };

                    await commandQueue
                        .AddAsync(new OrganizationUserCreateCommand(command.User, servicePrincipalUser))
                        .ConfigureAwait(false);
                }
            }

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }
}
