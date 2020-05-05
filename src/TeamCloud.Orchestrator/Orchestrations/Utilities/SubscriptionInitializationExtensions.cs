using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Activities;
using TeamCloud.Orchestrator.Templates;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    internal static class SubscriptionInitializationExtensions
    {
        private static readonly Version TargetSubscriptionVersion = new InitializeSubscriptionTemplate().TemplateVersion;

        internal static async Task InitializeSubscriptionAsync(this IDurableOrchestrationContext functionContext, Guid subscriptionId)
        {
            var currentSubscriptionVersion = await functionContext
                .GetSubscriptionVersionAsync(subscriptionId)
                .ConfigureAwait(true);

            if (currentSubscriptionVersion != TargetSubscriptionVersion)
            {
                await functionContext
                    .CallSubOrchestratorWithRetryAsync(nameof(SubscriptionInitializationOrchestration), subscriptionId)
                    .ConfigureAwait(true);
            }
        }
    }
}
