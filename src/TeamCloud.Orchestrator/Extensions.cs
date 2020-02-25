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
using TeamCloud.Orchestrator.Orchestrations.Locks;
using TeamCloud.Orchestrator.Orchestrations.Providers;

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

        internal static DateTime NextHour(this DateTime dateTime)
             => dateTime.Date.AddHours(dateTime.Hour + 1);

        internal static IEnumerable<Task<ICommandResult>> GetProviderCommandTasks(this List<Provider> providers, ICommand command, IDurableOrchestrationContext functionContext)
            => providers.Select(provider => functionContext.CallSubOrchestratorAsync<ICommandResult>(nameof(ProviderCommandOrchestration), (provider, command.GetProviderCommand(provider))));

        internal static void SetCustomStatus(this IDurableOrchestrationContext durableOrchestrationContext, object customStatusObject, ILogger log, Exception exception = null)
        {
            if (!durableOrchestrationContext.IsReplaying)
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
