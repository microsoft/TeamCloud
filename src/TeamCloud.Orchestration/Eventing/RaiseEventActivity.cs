using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud.Orchestration.Eventing
{
    public static class RaiseEventActivity
    {
        [FunctionName(nameof(RaiseEventActivity))]
        public static Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            [DurableClient] IDurableOrchestrationClient functionClient)
        {
            if (functionContext is null)
                throw new System.ArgumentNullException(nameof(functionContext));

            if (functionClient is null)
                throw new System.ArgumentNullException(nameof(functionClient));

            var (instanceId, eventName, eventData) = functionContext.GetInput<(string, string, object)>();

            return functionClient
                .RaiseEventAsync(instanceId, eventName, eventData);
        }
    }
}
