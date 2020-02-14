/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Orchestrator.Orchestrations;

namespace TeamCloud.Orchestrator.API
{
    public static class EternalTrigger
    {
        private static readonly IReadOnlyList<Type> eternalOrchestrationTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsClass && type.GetCustomAttribute<EternalOrchestrationAttribute>() != null)
            .ToList();

        [FunctionName(nameof(EternalTrigger))]
        public static void Run(
            [TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            [DurableClient] IDurableClient durableClient,
            ILogger log)
        {
            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var tasks = eternalOrchestrationTypes
                .Select(eternalOrchestrationType => EnsureEternalOrchestrationAsync(durableClient, eternalOrchestrationType, log))
                .ToArray();

            Task.WaitAll(tasks);
        }

        private static async Task EnsureEternalOrchestrationAsync(IDurableClient durableClient, Type eternalOrchestrationType, ILogger log)
        {
            var eternalOrchestrationAttribute = eternalOrchestrationType
                .GetCustomAttribute<EternalOrchestrationAttribute>();

            var eternalOrchestrationStatus = await durableClient
                .GetStatusAsync(eternalOrchestrationAttribute.InstanceId)
                .ConfigureAwait(false);

            if (eternalOrchestrationStatus?.IsFinalRuntimeStatus() ?? false)
            {
                // if the orchestration exists and reached a final status
                // we need to wipe it all state information

                log.LogWarning($"Purging history of eternal orchestration: {eternalOrchestrationAttribute.InstanceId}");

                await durableClient
                    .PurgeInstanceHistoryAsync(eternalOrchestrationAttribute.InstanceId)
                    .ConfigureAwait(false);
            }

            if (eternalOrchestrationStatus?.IsFinalRuntimeStatus() ?? true)
            {
                // if there is no orchestration status or the existing status
                // is in a final state we start a new orchestration instance

                var orchestrationName = eternalOrchestrationAttribute.OrchestrationName ?? eternalOrchestrationType.Name;

                log.LogInformation($"Starting new eternal orchstration '{orchestrationName}': {eternalOrchestrationAttribute.InstanceId}");

                await durableClient
                    .StartNewAsync(orchestrationName, eternalOrchestrationAttribute.InstanceId)
                    .ConfigureAwait(false);
            }
        }
    }
}
