/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command.Handlers;

public sealed class ProjectIdentityCommandHandler : CommandHandler,
    ICommandHandler<ProjectIdentityCreateCommand>,
    ICommandHandler<ProjectIdentityUpdateCommand>,
    ICommandHandler<ProjectIdentityDeleteCommand>
{
    private readonly IProjectIdentityRepository projectIdentityRepository;
    private readonly IGraphService graphService;

    public ProjectIdentityCommandHandler(IProjectIdentityRepository projectIdentityRepository, IGraphService graphService)
    {
        this.projectIdentityRepository = projectIdentityRepository ?? throw new ArgumentNullException(nameof(projectIdentityRepository));
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
    }

    public override bool Orchestration => false;

    public async Task<ICommandResult> HandleAsync(ProjectIdentityCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();
        var projectIdentity = command.Payload;

        try
        {
            var servicePrincipal = await graphService
                .CreateServicePrincipalAsync(projectIdentity.Id)
                .ConfigureAwait(false);

            projectIdentity.ObjectId = servicePrincipal.Id;
            projectIdentity.TenantId = servicePrincipal.TenantId;
            projectIdentity.ClientId = servicePrincipal.AppId;
            projectIdentity.ClientSecret = servicePrincipal.Password;

            if (projectIdentity.RedirectUrls is not null)
            {
                projectIdentity.RedirectUrls = await graphService
                    .SetServicePrincipalRedirectUrlsAsync(projectIdentity.ObjectId.ToString(), projectIdentity.RedirectUrls)
                    .ConfigureAwait(false);
            }

            commandResult.Result = await projectIdentityRepository
                .AddAsync(projectIdentity)
                .ConfigureAwait(false);

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            await commandQueue
                .AddAsync(new ProjectIdentityDeleteCommand(command.User, projectIdentity))
                .ConfigureAwait(false);

            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(ProjectIdentityUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();
        var projectIdentity = command.Payload;

        try
        {
            if (projectIdentity.RedirectUrls is not null)
            {
                projectIdentity.RedirectUrls = await graphService
                    .SetServicePrincipalRedirectUrlsAsync(projectIdentity.ObjectId.ToString(), projectIdentity.RedirectUrls)
                    .ConfigureAwait(false);
            }

            projectIdentity = await projectIdentityRepository
                .SetAsync(projectIdentity)
                .ConfigureAwait(false);

            commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

    public async Task<ICommandResult> HandleAsync(ProjectIdentityDeleteCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();
        var projectIdentity = command.Payload;

        try
        {
            await graphService
                .DeleteServicePrincipalAsync(projectIdentity.Id)
                .ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc, CommandErrorSeverity.Warning);
        }

        try
        {
            projectIdentity = await projectIdentityRepository
                .RemoveAsync(projectIdentity)
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
