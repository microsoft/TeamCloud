/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Configuration.Options;
using TeamCloud.Model;
using TeamCloud.Model.Broadcast;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Handlers.Messaging
{
    public sealed class BroadcastCommandHandler : CommandHandler
    {
        private readonly Type[] broadcastCommandTypes = new Type[]
        {
            typeof(BroadcastDocumentCreateCommand<>),
            typeof(BroadcastDocumentUpdateCommand<>),
            typeof(BroadcastDocumentDeleteCommand<>)
        };

        private readonly AzureSignalROptions azureSignalROptions;

        public BroadcastCommandHandler(AzureSignalROptions azureSignalROptions)
        {
            this.azureSignalROptions = azureSignalROptions ?? throw new ArgumentNullException(nameof(azureSignalROptions));
        }

        public override bool CanHandle(ICommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return command.GetType().IsGenericType
                && broadcastCommandTypes.Contains(command.GetType().GetGenericTypeDefinition());
        }

        public override async Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            if (CanHandle(command) && command.Payload is IContainerDocument containerDocument)
            {
                try
                {
                    using var loggerFactory = new PassthroughLoggerFactory(log);
                    using var serviceManager = CreateServiceManager();

                    var hubContext = await serviceManager
                        .CreateHubContextAsync(ResolveHubName(containerDocument))
                        .ConfigureAwait(false);

                    var broadcastMessage = new BroadcastMessage()
                    {
                        Action = commandResult.CommandAction.ToString().ToLowerInvariant(),
                        Timestamp = commandResult.LastUpdatedTime.GetValueOrDefault(DateTime.UtcNow),
                        Items = GetItems(containerDocument)

                    };

                    var broadcastPayload = TeamCloudSerialize.SerializeObject(broadcastMessage);

                    await hubContext.Clients.All
                        .SendAsync(command.CommandAction.ToString(), broadcastMessage)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    commandResult.Errors.Add(exc);
                }
            }

            return commandResult;

            static string ResolveHubName(object commandResultPayload) => commandResultPayload switch
            {
                IProjectContext projectContext => projectContext.GetHubName(),
                IOrganizationContext organizationContext => organizationContext.GetHubName(),
                _ => throw new NotSupportedException($"Unable to resolve hub name for command result payload of type '{commandResultPayload?.GetType()}'.")
            };

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

        private IServiceManager CreateServiceManager() => new ServiceManagerBuilder().WithOptions(option =>
        {
            option.ConnectionString = azureSignalROptions.ConnectionString;
            option.ServiceTransportType = ServiceTransportType.Transient;

        }).Build();

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
}
