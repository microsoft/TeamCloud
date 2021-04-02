/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.Configuration.Options;
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class BroadcastCommandHandler : CommandHandler
    {
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
                && command.GetType().GetGenericTypeDefinition() == typeof(BroadcastCommandResultCommand<>);
        }

        public override async Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            try
            {
                var commandPayload = (ICommandResult)command.Payload;

                using var loggerFactory = new PassthroughLoggerFactory(log);
                using var serviceManager = CreateServiceManager();

                var hubContext = await serviceManager
                    .CreateHubContextAsync(ResolveHubName(commandPayload.Result))
                    .ConfigureAwait(false);

                var broadcastMessage = commandPayload.ToBroadcastMessage();
                var broadcastPayload = TeamCloudSerialize.SerializeObject(broadcastMessage);

                await hubContext.Clients.All
                    .SendAsync(commandPayload.CommandAction.ToString(), broadcastMessage)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;

            static string ResolveHubName(object commandResultPayload) => commandResultPayload switch
            {
                IProjectContext projectContext => projectContext.GetHubName(),
                IOrganizationContext organizationContext => organizationContext.GetHubName(),
                _ => throw new NotSupportedException($"Unable to resolve hub name for command result payload of type '{commandResultPayload?.GetType()}'.")
            };
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
