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

public sealed class ComponentCreateCommandHandler : CommandHandler<ComponentCreateCommand>
{
    private readonly IComponentRepository componentRepository;
    private readonly IDeploymentScopeRepository deploymentScopeRepository;
    private readonly IAdapterProvider adapterProvider;
    private readonly IUserRepository userRepository;

    public ComponentCreateCommandHandler(IComponentRepository componentRepository,
                                         IDeploymentScopeRepository deploymentScopeRepository,
                                         IAdapterProvider adapterProvider,
                                         IUserRepository userRepository)
    {
        this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        this.deploymentScopeRepository = deploymentScopeRepository;
        this.adapterProvider = adapterProvider ?? throw new ArgumentNullException(nameof(adapterProvider));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public override bool Orchestration => false;

    public override async Task<ICommandResult> HandleAsync(ComponentCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
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

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(commandResult.Result.Organization, commandResult.Result.DeploymentScopeId)
                .ConfigureAwait(false);

            if (adapterProvider.GetAdapter(deploymentScope.Type) is IAdapterIdentity adapterIdentity)
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
                        Organization = commandResult.Result.Organization
                    };

                    servicePrincipalUser.EnsureProjectMembership(commandResult.Result.ProjectId, ProjectUserRole.Adapter);

                    await commandQueue
                        .AddAsync(new ProjectUserCreateCommand(command.User, servicePrincipalUser, commandResult.Result.ProjectId))
                        .ConfigureAwait(false);
                }
            }

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
