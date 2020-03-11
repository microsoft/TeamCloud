/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Activities;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities
{
    public static class ProjectCommandSerializationOrchestrator
    {
        [FunctionName(nameof(ProjectCommandSerializationOrchestrator))]
        public static async Task RunOrchestrator(
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
                        .CallActivityWithRetryAsync<bool>(nameof(ProjectCommandSerializationActivity), notification)
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






    }



}
