/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class ProviderSendExtensions
    {
        internal static async Task<TCommandResult> SendProviderCommandAsync<TCommand, TCommandResult>(this IDurableOrchestrationContext functionContext, TCommand command)
            where TCommand : IProviderCommand
            where TCommandResult : ICommandResult
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var providers = await functionContext
                .ListProvidersAsync()
                .ConfigureAwait(true);

            var providerTasks = providers.Select(p => functionContext.SendProviderCommandAsync<TCommand, TCommandResult>(command, p));

            var providerResults = await Task
                .WhenAll(providerTasks)
                .ConfigureAwait(true);

            var providerResult = (TCommandResult)command.CreateResult();

            providerResults.SelectMany(r => r.Errors).ToList().ForEach(e => providerResult.Errors.Add(e));

            return providerResult;
        }


        internal static async Task<TCommandResult> SendProviderCommandAsync<TCommand, TCommandResult>(this IDurableOrchestrationContext functionContext, TCommand command, ProviderDocument provider)
            where TCommand : IProviderCommand
            where TCommandResult : ICommandResult
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            command.ProviderId = provider.Id;

            var providerResult = (TCommandResult)await functionContext
                .CallSubOrchestratorWithRetryAsync<ICommandResult>(nameof(ProviderSendOrchestration), (command, provider))
                .ConfigureAwait(true);

            if (providerResult is null)
            {
                providerResult = (TCommandResult)command.CreateResult();
                providerResult.Errors.Add(new NullReferenceException($"Provider '{provider.Id}' returned no result for command '{command.CommandId}'"));
            }

            return providerResult;
        }


        internal static Task<IDictionary<string, ICommandResult>> SendProviderCommandAsync<TCommand>(this IDurableOrchestrationContext functionContext, TCommand command, ProjectDocument project, bool failFast = false)
            where TCommand : IProviderCommand
            => functionContext.SendProviderCommandAsync<TCommand, ICommandResult>(command, project, failFast);


        internal static async Task<IDictionary<string, TCommandResult>> SendProviderCommandAsync<TCommand, TCommandResult>(this IDurableOrchestrationContext functionContext, TCommand command, ProjectDocument project, bool failFast = false)
            where TCommand : IProviderCommand
            where TCommandResult : ICommandResult
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (project is null && command is IProviderCommand<Model.Data.Project>)
                throw new InvalidOperationException("Must pass original ProjectDocument (internal) for ProviderCommands with a payload of type Project (external).");

            if (project is null && !string.IsNullOrEmpty(command.ProjectId))
            {
                project = await functionContext
                    .GetProjectAsync(command.ProjectId, allowUnsafe: true)
                    .ConfigureAwait(true);
            }

            var providerBatches = await functionContext
                .CallActivityWithRetryAsync<IEnumerable<IEnumerable<ProviderDocument>>>(nameof(CommandProviderActivity), project)
                .ConfigureAwait(true);

            var commandResults = Enumerable.Empty<KeyValuePair<string, TCommandResult>>();

            foreach (var providerBatch in providerBatches)
            {
                foreach (var commandResult in commandResults.Where(cr => cr.Value is ICommandResult<ProviderOutput>))
                {
                    var commandResultOutput = commandResult.Value as ICommandResult<ProviderOutput>;

                    command.Results.TryAdd(commandResult.Key, commandResultOutput?.Result?.Properties ?? new Dictionary<string, string>());
                }

                var providerTasks = providerBatch.Select(async provider =>
                {
                    var providerResult = await functionContext
                        .SendProviderCommandAsync<TCommand, TCommandResult>(command, provider)
                        .ConfigureAwait(true);

                    return new KeyValuePair<string, TCommandResult>(provider.Id, providerResult);
                });

                commandResults = commandResults.Concat(await Task
                    .WhenAll(providerTasks)
                    .ConfigureAwait(true));

                if (failFast && commandResults.Any(cr => cr.Value.Errors.Any()))
                    break;
            }

            return new Dictionary<string, TCommandResult>(commandResults);
        }
    }
}
