/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    internal static class CommandAuditExtension
    {
        public static Task AuditAsync(this IDurableOrchestrationContext functionContext, IOrchestratorCommand command, ICommandResult commandResult = default)
        {
            if (command is null)
                throw new System.ArgumentNullException(nameof(command));

            return functionContext.CallActivityWithRetryAsync(nameof(CommandAuditActivity), (default(Provider), command, commandResult));
        }

        public static Task AuditAsync(this IDurableOrchestrationContext functionContext, IEnumerable<Provider> providers, IProviderCommand command, ICommandResult commandResult = default)
        {
            if (providers is null)
                throw new System.ArgumentNullException(nameof(providers));

            if (command is null)
                throw new System.ArgumentNullException(nameof(command));

            var tasks = providers
                .Select(provider => functionContext.AuditAsync(provider, command, commandResult));

            return Task
                .WhenAll(tasks);
        }

        public static Task AuditAsync(this IDurableOrchestrationContext functionContext, Provider provider, IProviderCommand command, ICommandResult commandResult = default)
        {
            if (provider is null)
                throw new System.ArgumentNullException(nameof(provider));

            if (command is null)
                throw new System.ArgumentNullException(nameof(command));

            return functionContext.CallActivityWithRetryAsync(nameof(CommandAuditActivity), (provider, command, commandResult));
        }
    }
}
