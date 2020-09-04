/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestration.Auditing
{
    public static class CommandAuditExtension
    {
        public static Task AuditAsync(this IDurableOrchestrationContext functionContext, ICommand command, ICommandResult commandResult = default, IProvider provider = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return functionContext.CallActivityWithRetryAsync(nameof(CommandAuditActivity), (command, commandResult, provider?.Id));
        }
    }
}
