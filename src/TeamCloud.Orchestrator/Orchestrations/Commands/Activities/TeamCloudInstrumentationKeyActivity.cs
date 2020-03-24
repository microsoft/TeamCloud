using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
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
}
