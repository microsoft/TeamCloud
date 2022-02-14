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

public sealed class ProjectUserCommandHandler : CommandHandler,
      ICommandHandler<ProjectUserCreateCommand>,
      ICommandHandler<ProjectUserUpdateCommand>,
      ICommandHandler<ProjectUserDeleteCommand>
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IProjectRepository projectRepository;
    private readonly IUserRepository userRepository;
    private readonly IComponentRepository componentRepository;
    private readonly IAzureSessionService azureSessionService;
    private readonly TeamCloudEndpointOptions endpointOptions;

    public ProjectUserCommandHandler(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IUserRepository userRepository, IComponentRepository componentRepository, IAzureSessionService azureSessionService, TeamCloudEndpointOptions endpointOptions)
    {
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        this.endpointOptions = endpointOptions ?? throw new ArgumentNullException(nameof(endpointOptions));
    }

    public override bool Orchestration => false;

    public async Task<ICommandResult> HandleAsync(ProjectUserCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
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

            await SendWelcomeMessageAsync(command, commandQueue, currentUser)
                .ConfigureAwait(false);

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }
        finally
        {
            await UpdateComponentsAsync(command.User, command.ProjectId, commandQueue)
                .ConfigureAwait(false);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(ProjectUserUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            commandResult.Result = await userRepository
                .SetAsync(command.Payload)
                .ConfigureAwait(false);

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }
        finally
        {
            await UpdateComponentsAsync(command.User, command.ProjectId, commandQueue)
                .ConfigureAwait(false);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(ProjectUserDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
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
        finally
        {
            await UpdateComponentsAsync(command.User, command.ProjectId, commandQueue)
                .ConfigureAwait(false);
        }

        return commandResult;
    }

    private async Task UpdateComponentsAsync(User user, string projectId, IAsyncCollector<ICommand> commandQueue)
    {
        await foreach (var component in componentRepository.ListAsync(projectId))
        {
            var command = new ComponentUpdateCommand(user, component);

            await commandQueue
                .AddAsync(command)
                .ConfigureAwait(false);
        }
    }

    private async Task SendWelcomeMessageAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, User userOld)
    {
        if (command.Payload is User userNew)
        {
            var projects = await userNew.ProjectMemberships
                .Select(pm => pm.ProjectId)
                .Except(userOld?.ProjectMemberships.Select(pm => pm.ProjectId) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase)
                .Select(pid => projectRepository.GetAsync(userNew.Organization, pid, true))
                .WhenAll()
                .ConfigureAwait(false);

            if (projects.Any())
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

                await projects
                    .Select(project => SendWelcomeMessageAsync(command.User, organization, project, userNew))
                    .WhenAll()
                    .ConfigureAwait(false);
            }
        }

        Task SendWelcomeMessageAsync(User sender, Organization organization, Project project, User user)
        {
            var message = NotificationMessage.Create<WelcomeMessage>(user);

            message.Merge(new WelcomeMessageData()
            {
                Organization = organization,
                Project = project,
                User = user,
                PortalUrl = endpointOptions.Portal
            });

            return commandQueue.AddAsync(new NotificationSendMailCommand<WelcomeMessage>(sender, message));
        }
    }
}
