using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class ProjectCommandSerializationEntity
    {
        [FunctionName(nameof(ProjectCommandSerializationEntity))]
        public static async Task RunEntity(
            [EntityTrigger] IDurableEntityContext functionContext,
            [DurableClient] IDurableClient durableClient)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var activeCommand = functionContext.GetState<ICommand>();
            var pendingCommand = functionContext.GetInput<ICommand>();

            functionContext.SetState(pendingCommand);

            var activeCommandId = activeCommand?.CommandId;

            if (activeCommandId.HasValue)
            {
                var status = await durableClient
                    .GetStatusAsync(activeCommandId.Value.ToString())
                    .ConfigureAwait(false);

                if (status?.IsFinalRuntimeStatus() ?? true)
                    activeCommandId = null;
            }

            functionContext.Return(activeCommandId?.ToString());
        }
    }
}
