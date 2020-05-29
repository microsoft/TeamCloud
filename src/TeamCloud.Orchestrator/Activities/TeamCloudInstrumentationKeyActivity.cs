/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public static class TeamCloudInstrumentationKeyActivity
    {
        [FunctionName(nameof(TeamCloudInstrumentationKeyActivity))]
        public static string RunActivity(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

            if (string.IsNullOrWhiteSpace(instrumentationKey))
                instrumentationKey = Guid.Empty.ToString();

            return instrumentationKey;
        }
    }

    internal static class TeamCloudInstrumentationKeyExtension
    {
        public static Task<string> GetInstrumentationKeyAsync(this IDurableOrchestrationContext functionContext)
            => functionContext.CallActivityWithRetryAsync<string>(nameof(TeamCloudInstrumentationKeyActivity), null);
    }
}
