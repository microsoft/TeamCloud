/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Orchestrations.Commands
{
    public static class ProjectCommandSerialization
    {
        [FunctionName(nameof(ProjectCommandSerializationOrchestrator))]
        public static async Task ProjectCommandSerializationOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var command = context.GetInput<ICommand>();

            if (command.ProjectId.HasValue)
            {
                var commandEntityId = new EntityId(nameof(ProjectCommandSerializationEntity), command.ProjectId.ToString());
                var commandDependencyId = default(string);

                using (await context.LockAsync(commandEntityId).ConfigureAwait(true))
                {
                    // register the new command is a critical section and needs a lock
                    // if there's an dependency it's guid is returned; otherwise null

                    commandDependencyId = await context
                        .CallEntityAsync<string>(commandEntityId, null, command)
                        .ConfigureAwait(true);
                }

                if (Guid.TryParse(commandDependencyId, out var correlationId))
                {
                    var notification = new ProjectCommandNotification()
                    {
                        InstanceId = context.InstanceId,
                        CorrelationId = correlationId
                    };

                    var waitForExternalEvent = await context
                        .CallActivityAsync<bool>(nameof(ProjectCommandSerializationActivity), notification)
                        .ConfigureAwait(true);

                    if (waitForExternalEvent)
                    {
                        context
                            .CreateReplaySafeLogger(log)
                            .LogWarning($"{notification.InstanceId} - Waiting for command {notification.CorrelationId}");

                        await context
                            .WaitForExternalEvent(notification.CorrelationId.ToString())
                            .ConfigureAwait(true);

                        context
                            .CreateReplaySafeLogger(log)
                            .LogWarning($"{notification.InstanceId} - Resuming");
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
                .GetStatusAsync(notification.CorrelationId.ToString())
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
            [EntityTrigger] IDurableEntityContext context,
            [DurableClient] IDurableClient durableClient)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var oldCommand = context.GetState<ICommand>();
            var newCommand = context.GetInput<ICommand>();

            context.SetState(newCommand);

            var correlationId = oldCommand?.CommandId;

            if (correlationId.HasValue)
            {
                var status = await durableClient
                    .GetStatusAsync(correlationId.Value.ToString())
                    .ConfigureAwait(false);

                if (status?.IsFinalRuntimeStatus() ?? true)
                {
                    correlationId = null;
                }
            }

            context.Return(correlationId?.ToString());
        }
    }
}
