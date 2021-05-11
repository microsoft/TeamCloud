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
using TeamCloud.Adapters;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Handlers;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.API;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class DeploymentScopeCommandHandler : CommandHandler,
        ICommandHandler<DeploymentScopeCreateCommand>,
        ICommandHandler<DeploymentScopeUpdateCommand>,
        ICommandHandler<DeploymentScopeDeleteCommand>,
        ICommandHandler<DeploymentScopeAuthorizeCommand>
    {
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IAuthorizationSessionClient authorizationSessionClient;
        private readonly IAdapter[] adapters;
        private readonly IFunctionsHost functionsHost;

        public DeploymentScopeCommandHandler(IDeploymentScopeRepository deploymentScopeRepository, IAuthorizationSessionClient authorizationSessionClient, IAdapter[] adapters, IFunctionsHost functionsHost)
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.authorizationSessionClient = authorizationSessionClient ?? throw new ArgumentNullException(nameof(authorizationSessionClient));
            this.adapters = adapters ?? Array.Empty<IAdapter>();
            this.functionsHost = functionsHost ?? FunctionsHost.Default;
        }

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

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

        public async Task<ICommandResult> HandleAsync(DeploymentScopeUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await deploymentScopeRepository
                    .SetAsync(command.Payload)
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }

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

        public async Task<ICommandResult> HandleAsync(DeploymentScopeAuthorizeCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = command.Payload;

                var adapter = adapters
                    .FirstOrDefault(a => a.Supports(command.Payload));

                if (adapter is null)
                {
                    throw new NullReferenceException($"Could not find adapter for {command.Payload}");
                }
                else if (adapter.Supports(command.Payload) && adapter is IAdapterAuthorize adapterAuthorize)
                {
                    await adapterAuthorize
                        .CreateSessionAsync(command.Payload)
                        .ConfigureAwait(false);

                    commandResult.Result.AuthorizeUrl = await FunctionsEnvironment
                        .GetFunctionUrlAsync(nameof(AuthorizationTrigger.Authorize), functionsHost, replaceToken: (token) => AuthorizationTrigger.ResolveTokenValue(token, command.Payload))
                        .ConfigureAwait(false);

                    commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
                }
                else
                {
                    throw new NotSupportedException($"Authorization not supported for {command.Payload}");
                }

            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }
    }
}
