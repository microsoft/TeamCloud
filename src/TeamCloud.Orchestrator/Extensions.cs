/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Commands;
using TeamCloud.Orchestrator.Orchestrations.Locks;
using TeamCloud.Orchestrator.Orchestrations.Projects.Activities;

namespace TeamCloud.Orchestrator
{
    internal static class Extensions
    {
        private static readonly int[] FinalRuntimeStatus = new int[]
        {
            (int) OrchestrationRuntimeStatus.Completed,
            (int) OrchestrationRuntimeStatus.Failed,
            (int) OrchestrationRuntimeStatus.Canceled,
            (int) OrchestrationRuntimeStatus.Terminated
        };

        internal static bool IsFinalRuntimeStatus(this DurableOrchestrationStatus status)
        {
            if (status is null) throw new ArgumentNullException(nameof(status));

            return FinalRuntimeStatus.Contains((int)status.RuntimeStatus);
        }

        internal static Task<IDisposable> LockAsync(this IDurableOrchestrationContext functionContext, IContainerDocument containerDocument)
            => functionContext.LockAsync(containerDocument.GetEntityId());

        internal static EntityId GetEntityId<T>(this T document)
            where T : class, IContainerDocument
            => new EntityId(nameof(DocumentEntityLock), $"{document.Id}@{document.GetType()}");

        internal static ICommandResult CreateResult(this ICommand command, DurableOrchestrationStatus orchestrationStatus)
        {
            var result = (orchestrationStatus.Output?.HasValues ?? false) ? orchestrationStatus.Output.ToObject<ICommandResult>() : command.CreateResult();

            result.CreatedTime = orchestrationStatus.CreatedTime;
            result.LastUpdatedTime = orchestrationStatus.LastUpdatedTime;
            result.RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus;
            result.CustomStatus = orchestrationStatus.CustomStatus?.ToString();

            return result;
        }

        internal static ICommandResult GetCommandResult(this DurableOrchestrationStatus orchestrationStatus)
        {
            if (orchestrationStatus.Output?.HasValues ?? false)
            {
                var result = orchestrationStatus.Output.ToObject<ICommandResult>();

                result.CreatedTime = orchestrationStatus.CreatedTime;
                result.LastUpdatedTime = orchestrationStatus.LastUpdatedTime;
                result.RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus;
                result.CustomStatus = orchestrationStatus.CustomStatus?.ToString();

                return result;
            }
            else if (orchestrationStatus.Input?.HasValues ?? false)
            {
                var command = orchestrationStatus.Input.ToObject<OrchestratorCommandMessage>()?.Command;

                return command?.CreateResult(orchestrationStatus);
            }

            return null;
        }


        internal static IDictionary<string, string> Merge(this IDictionary<string, string> instance, IDictionary<string, string> merge, params IDictionary<string, string>[] additionalMerges)
        {
            var keyValuePairs = instance.Concat(merge);

            foreach (var additionalMerge in additionalMerges)
                keyValuePairs = keyValuePairs.Concat(additionalMerge);

            return keyValuePairs
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Last().Value);
        }

        internal static void Merge(this Project project, IDictionary<string, ICommandResult<ProviderOutput>> commandResults)
        {
            foreach (var commandResult in commandResults)
                project.Merge(commandResult.Key, commandResult.Value);
        }

        internal static void Merge(this Project project, string providerId, ICommandResult<ProviderOutput> commandResult)
        {
            if (project.Outputs.TryGetValue(providerId, out var providerProperties))
            {
                project.Outputs[providerId] = providerProperties.Merge(commandResult.Result.Properties);
            }
            else
            {
                project.Outputs.Add(providerId, commandResult.Result.Properties);
            }
        }

        internal static DateTime NextHour(this DateTime dateTime)
             => dateTime.Date.AddHours(dateTime.Hour + 1);

        internal static Task<ICommandResult> SendCommandAsync(this IDurableOrchestrationContext functionContext, ICommand command, Provider provider)
            => functionContext.SendCommandAsync<ICommandResult>(command, provider);

        internal static async Task<TCommandResult> SendCommandAsync<TCommandResult>(this IDurableOrchestrationContext functionContext, ICommand command, Provider provider)
            where TCommandResult : ICommandResult
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var providerResult = (TCommandResult)await functionContext
                .CallSubOrchestratorWithRetryAsync<ICommandResult>(nameof(CommandSendOrchestration), (command, provider))
                .ConfigureAwait(true);

            return providerResult;
        }

        internal static Task<IDictionary<string, ICommandResult>> SendCommandAsync(this IDurableOrchestrationContext functionContext, ICommand command, Project project = null, TeamCloudInstance teamCloud = null)
            => functionContext.SendCommandAsync<ICommandResult>(command, project, teamCloud);

        internal static async Task<IDictionary<string, TCommandResult>> SendCommandAsync<TCommandResult>(this IDurableOrchestrationContext functionContext, ICommand command, Project project = null, TeamCloudInstance teamCloud = null)
            where TCommandResult : ICommandResult
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            teamCloud ??= await functionContext
                .CallActivityWithRetryAsync<TeamCloudInstance>(nameof(TeamCloudGetActivity), project?.TeamCloudId)
                .ConfigureAwait(true);

            IEnumerable<Provider> providers = teamCloud.Providers;

            if (command.ProjectId.HasValue)
            {
                project ??= await functionContext
                    .CallActivityWithRetryAsync<Project>(nameof(ProjectGetActivity), command.ProjectId.Value)
                    .ConfigureAwait(true);

                providers = teamCloud.ProvidersFor(project);
            }

            if (!(command.ProjectId?.Equals(project.Id) ?? true))
                throw new ArgumentException("The provided project doesn't match the project referenced by the command", nameof(project));

            if (!(project?.TeamCloudId.Equals(teamCloud.Id, StringComparison.Ordinal) ?? true))
                throw new ArgumentException("The provided TeamCloud instance doesn't match the instance referenced by project", nameof(teamCloud));

            var providerResults = Enumerable.Empty<KeyValuePair<string, TCommandResult>>();

            if (providers.Any())
            {
                var providerTasks = providers
                    .Select(async provider =>
                    {
                        var providerResult = (TCommandResult)await functionContext
                            .SendCommandAsync(command, provider)
                            .ConfigureAwait(true);

                        return new KeyValuePair<string, TCommandResult>(provider.Id, providerResult);
                    });

                providerResults = await Task
                    .WhenAll(providerTasks)
                    .ConfigureAwait(true);
            }

            return new Dictionary<string, TCommandResult>(providerResults);
        }

        internal static void SetCustomStatus(this IDurableOrchestrationContext durableOrchestrationContext, object customStatusObject, ILogger log, Exception exception = null)
        {
            durableOrchestrationContext.SetCustomStatus(customStatusObject);

            var customStatusMessage = customStatusObject is string
                ? customStatusObject.ToString()
                : JsonConvert.SerializeObject(customStatusObject, Formatting.None);

            if (log != null)
            {
                if (exception is null)
                    durableOrchestrationContext.CreateReplaySafeLogger(log).LogInformation($"{durableOrchestrationContext.InstanceId} - CUSTOM STATUS: {customStatusMessage}");
                else
                    durableOrchestrationContext.CreateReplaySafeLogger(log).LogError(exception, $"{durableOrchestrationContext.InstanceId} - CUSTOM STATUS: {customStatusMessage}");
            }
        }
    }
}
