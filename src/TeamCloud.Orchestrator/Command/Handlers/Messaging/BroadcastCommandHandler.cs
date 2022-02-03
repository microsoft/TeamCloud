/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Configuration.Options;
using TeamCloud.Data;
using TeamCloud.Model;
using TeamCloud.Model.Broadcast;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Handlers.Messaging;

public sealed class BroadcastCommandHandler : CommandHandler
{
    private readonly Type[] broadcastCommandTypes = new Type[]
    {
            typeof(BroadcastDocumentCreateCommand<>),
            typeof(BroadcastDocumentUpdateCommand<>),
            typeof(BroadcastDocumentDeleteCommand<>)
    };

    private readonly AzureSignalROptions azureSignalROptions;
    private readonly IProjectRepository projectRepository;

    public BroadcastCommandHandler(AzureSignalROptions azureSignalROptions, IProjectRepository projectRepository)
    {
        this.azureSignalROptions = azureSignalROptions ?? throw new ArgumentNullException(nameof(azureSignalROptions));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
    }

    public override bool Orchestration => false;

    public override bool CanHandle(ICommand command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        return command.Payload is IContainerDocument
            && command.GetType().IsGenericType
            && broadcastCommandTypes.Contains(command.GetType().GetGenericTypeDefinition());
    }

    public override async Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        var commandResult = command.CreateResult();

        try
        {
            if (CanHandle(command))
            {
                var containerDocument = (IContainerDocument)command.Payload;

                using var loggerFactory = new PassthroughLoggerFactory(log);
                using var serviceManager = CreateServiceManager();

                var broadcastMessage = new BroadcastMessage()
                {
                    Action = commandResult.CommandAction.ToString().ToLowerInvariant(),
                    Timestamp = commandResult.LastUpdatedTime.GetValueOrDefault(DateTime.UtcNow),
                    Items = GetItems(containerDocument)
                };

                var broadcastPayload = TeamCloudSerialize.SerializeObject(broadcastMessage);

                await foreach (var hubName in ResolveHubNamesAsync(containerDocument))
                {
                    var hubContext = await serviceManager
                        .CreateHubContextAsync(hubName, CancellationToken.None)
                        .ConfigureAwait(false);

                    var negotiation = await hubContext
                        .NegotiateAsync()
                        .ConfigureAwait(false);

                    await hubContext.Clients.All
                        .SendAsync(command.CommandAction.ToString(), broadcastMessage)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                throw new NotImplementedException($"Missing orchestrator command handler implementation ICommandHandler<{command.GetTypeName(prettyPrint: true)}> at {GetType()}");
            }
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;

        async IAsyncEnumerable<string> ResolveHubNamesAsync(object commandResultPayload)
        {
            yield return commandResultPayload switch
            {
                IProjectContext projectContext => projectContext.GetHubName(),
                IOrganizationContext organizationContext => organizationContext.GetHubName(),
                Organization organization => organization.GetHubName(),
                _ => throw new NotSupportedException($"Unable to resolve hub name for command result payload of type '{commandResultPayload?.GetType()}'.")
            };

            if (commandResultPayload is User user)
            {
                foreach (var membership in user.ProjectMemberships)
                {
                    var project = await projectRepository
                        .GetAsync(user.Organization, membership.ProjectId)
                        .ConfigureAwait(false);

                    yield return project.GetHubName();
                }
            }
        }
    }

    public static IEnumerable<BroadcastMessage.Item> GetItems(params IContainerDocument[] containerDocuments)
    {
        foreach (var containerDocument in containerDocuments)
        {
            yield return new BroadcastMessage.Item()
            {
                Id = containerDocument.Id,
                Type = containerDocument.GetType().Name.ToLowerInvariant(),
                Organization = (containerDocument as IOrganizationContext)?.Organization,
                Project = (containerDocument as IProjectContext)?.ProjectId,
                Component = (containerDocument as IComponentContext)?.ComponentId,
                Slug = (containerDocument as ISlug)?.Slug,
                ETag = containerDocument.ETag,
                Timestamp = containerDocument.Timestamp
            };
        }
    }

    private ServiceManager CreateServiceManager() => new ServiceManagerBuilder().WithOptions(option =>
    {
        option.ConnectionString = azureSignalROptions.ConnectionString;
        option.ServiceTransportType = ServiceTransportType.Transient;

    }).BuildServiceManager();

    private sealed class PassthroughLoggerFactory : ILoggerFactory
    {
        private readonly ILogger log;

        public PassthroughLoggerFactory(ILogger log)
        {
            this.log = log ?? NullLogger.Instance;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            // nothing to dispose
        }

        public ILogger CreateLogger(string categoryName)
        {
            return log;
        }

        public void Dispose()
        {
            // nothing to dispose
        }
    }
}
