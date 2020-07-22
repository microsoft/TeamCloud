/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class ProviderSendExtensions
    {
        internal static async Task<TCommandResult> SendProviderCommandAsync<TCommand, TCommandResult>(this IDurableOrchestrationContext functionContext, TCommand command, Provider provider)
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


        internal static Task<IDictionary<string, ICommandResult>> SendProviderCommandAsync<TCommand>(this IDurableOrchestrationContext functionContext, TCommand command, Project project, bool failFast = false)
            where TCommand : IProviderCommand
            => functionContext.SendProviderCommandAsync<TCommand, ICommandResult>(command, project, failFast);


        internal static async Task<IDictionary<string, TCommandResult>> SendProviderCommandAsync<TCommand, TCommandResult>(this IDurableOrchestrationContext functionContext, TCommand command, Project project, bool failFast = false)
            where TCommand : IProviderCommand
            where TCommandResult : ICommandResult
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (project is null && command is IProviderCommand<Model.Data.Project>)
                throw new InvalidOperationException("Must pass original Project (internal) for ProviderCommands with a payload of type Project (external).");

            if (project is null && !string.IsNullOrEmpty(command.ProjectId))
            {
                project = await functionContext
                    .GetProjectAsync(command.ProjectId, allowUnsafe: true)
                    .ConfigureAwait(true);
            }

            var providerBatches = await functionContext
                .CallActivityWithRetryAsync<IEnumerable<IEnumerable<Provider>>>(nameof(CommandProviderActivity), project)
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
