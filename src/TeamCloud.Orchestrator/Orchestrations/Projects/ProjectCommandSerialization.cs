/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;

namespace TeamCloud.Orchestrator.Orchestrations.Projects
{
    public static class ProjectCommandSerialization
    {
        [FunctionName(nameof(ProjectCommandSerializationOrchestrator))]
        public static async Task ProjectCommandSerializationOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<ICommand>();

            if (command.ProjectId.HasValue)
            {
                var commandEntityId = new EntityId(nameof(ProjectCommandSerializationEntity), command.ProjectId.ToString());
                var activeCommandId = default(string);

                using (await context.LockAsync(commandEntityId).ConfigureAwait(true))
                {
                    // register the new command is a critical section and needs a lock
                    // if there's an dependency it's guid is returned; otherwise null

                    activeCommandId = await context
                        .CallEntityAsync<string>(commandEntityId, null, command)
                        .ConfigureAwait(true);
                }

                if (Guid.TryParse(activeCommandId, out var activeCommandGuid))
                {
                    var notification = new ProjectCommandNotification
                    {
                        PendingInstanceId = context.InstanceId,
                        ActiveCommandId = activeCommandGuid
                    };

                    var waitForExternalEvent = await context
                        .CallActivityAsync<bool>(nameof(ProjectCommandSerializationActivity), notification)
                        .ConfigureAwait(true);

                    if (waitForExternalEvent)
                    {
                        context
                            .CreateReplaySafeLogger(log)
                            .LogWarning($"{notification.PendingInstanceId} - Waiting for command {notification.ActiveCommandId}");

                        await context
                            .WaitForExternalEvent(notification.ActiveCommandId.ToString())
                            .ConfigureAwait(true);

                        context
                            .CreateReplaySafeLogger(log)
                            .LogWarning($"{notification.PendingInstanceId} - Resuming");
                    }
                }
            }
        }


        [FunctionName(nameof(ProjectCommandSerializationActivity))]
        public static async Task<bool> ProjectCommandSerializationActivity(
            [ActivityTrigger] ProjectCommandNotification notification,
            [DurableClient] IDurableClient durableClient)
        {
            if (notification is null)
                throw new ArgumentNullException(nameof(notification));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var status = await durableClient
                .GetStatusAsync(notification.ActiveCommandId.ToString())
                .ConfigureAwait(false);

            if (!status.IsFinalRuntimeStatus())
            {
                _ = await durableClient
                    .StartNewAsync(nameof(ProjectCommandMonitoring.ProjectCommandMonitoringOrchestrator), notification)
                    .ConfigureAwait(false);

                return true; // command monitoring started
            }

            return false; // no command monitoring started
        }


        [FunctionName(nameof(ProjectCommandSerializationEntity))]
        public static async Task ProjectCommandSerializationEntity(
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


    internal static class ProjectCommandExtensions
    {
        internal static Task WaitForProjectCommandsAsync(this IDurableOrchestrationContext context, ICommand command)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (command.ProjectId.HasValue)
                return context.CallSubOrchestratorAsync(nameof(ProjectCommandSerialization.ProjectCommandSerializationOrchestrator), command);

            return Task.CompletedTask;
        }
    }
}
