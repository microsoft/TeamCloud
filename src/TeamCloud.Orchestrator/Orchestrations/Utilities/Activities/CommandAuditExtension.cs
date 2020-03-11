using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    internal static class CommandAuditExtension
    {
        public static Task AuditAsync(this IDurableOrchestrationContext functionContext, Provider provider, ICommand command, ICommandResult commandResult = null)
        {
            if (provider is null)
                throw new System.ArgumentNullException(nameof(provider));

            if (command is null)
                throw new System.ArgumentNullException(nameof(command));

            return functionContext.CallActivityWithRetryAsync(nameof(CommandAuditActivity), (functionContext.InstanceId, provider, command, commandResult));
        }
    }
}
