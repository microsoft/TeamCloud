/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Messaging;
using TeamCloud.Orchestrator.Options;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class OrganizationUserCommandHandler : CommandHandler,
      ICommandHandler<OrganizationUserCreateCommand>,
      ICommandHandler<OrganizationUserUpdateCommand>,
      ICommandHandler<OrganizationUserDeleteCommand>
{
    private readonly IUserRepository userRepository;
    private readonly IOrganizationRepository organizationRepository;
    private readonly IAzureSessionService azureSessionService;
    private readonly TeamCloudEndpointOptions endpointOptions;

    public OrganizationUserCommandHandler(IUserRepository userRepository, IOrganizationRepository organizationRepository, IAzureSessionService azureSessionService, TeamCloudEndpointOptions endpointOptions)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        this.endpointOptions = endpointOptions ?? throw new ArgumentNullException(nameof(endpointOptions));
    }

    public override bool Orchestration => false;

    public async Task<ICommandResult> HandleAsync(OrganizationUserCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await userRepository
                .AddAsync(command.Payload)
                .ConfigureAwait(false);

            try
            {
                await SendAlternateIdentityMessageAsync(command, commandQueue, null)
                    .ConfigureAwait(false);
            }
            catch (Exception mailExc)
            {
                commandResult.Errors.Add(mailExc, CommandErrorSeverity.Warning);
            }
            finally
            {
                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(OrganizationUserUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            var currentUser = await userRepository
                .GetAsync(command.Payload.Organization, command.Payload.Id)
                .ConfigureAwait(false);

            commandResult.Result = await userRepository
                .SetAsync(command.Payload)
                .ConfigureAwait(false);

            try
            {
                await SendAlternateIdentityMessageAsync(command, commandQueue, currentUser)
                    .ConfigureAwait(false);
            }
            catch (Exception mailExc)
            {
                commandResult.Errors.Add(mailExc, CommandErrorSeverity.Warning);
            }
            finally
            {
                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(OrganizationUserDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await userRepository
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

    private async Task SendAlternateIdentityMessageAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, User userOld)
    {
        if (command.Payload is User userNew)
        {
            var alternateIdentities = userNew.AlternateIdentities.Keys
                .Except(userOld?.AlternateIdentities.Keys ?? Enumerable.Empty<DeploymentScopeType>())
                .Select(deploymentScopeType => deploymentScopeType.ToString(prettyPrint: true));

            if (alternateIdentities.Any())
            {
                var tenantId = await azureSessionService
                    .GetTenantIdAsync()
                    .ConfigureAwait(false);

                var organization = await organizationRepository
                    .GetAsync(tenantId.ToString(), userNew.Organization, expand: true)
                    .ConfigureAwait(false);

                userNew = await userRepository
                    .ExpandAsync(userNew)
                    .ConfigureAwait(false);

                var message = NotificationMessage.Create<AlternateIdentityMessage>(userNew);

                message.Merge(new AlternateIdentityMessageData()
                {
                    Organization = organization,
                    Services = alternateIdentities.ToArray(),
                    User = userNew,
                    PortalUrl = endpointOptions.Portal
                });

                await commandQueue
                    .AddAsync(new NotificationSendMailCommand<AlternateIdentityMessage>(command.User, message))
                    .ConfigureAwait(false);
            }
        }
    }
}
