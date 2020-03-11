/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    internal static class ProjectCommandSerializationExtensions
    {
        internal static Task WaitForProjectCommandsAsync(this IDurableOrchestrationContext context, ICommand command)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (command.ProjectId.HasValue)
                return context.CallSubOrchestratorWithRetryAsync(nameof(ProjectCommandSerializationOrchestrator), command);

            return Task.CompletedTask;
        }
    }
}
