/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Entities
{
    public static class CommandMetricExtensions
    {
        internal static IDisposable TrackCommandMetrics(this IDurableOrchestrationContext orchestrationContext, ICommand command)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            orchestrationContext.SignalEntity(GetEntityId(command), CommandMetricEntity.IncrementCount);

            return new CommandMetricScope(() => orchestrationContext.SignalEntity(GetEntityId(command), CommandMetricEntity.DecrementCount));
        }

        internal static void ResetCommandMetrics(this IDurableOrchestrationContext orchestrationContext, ICommand command)
        {
            if (orchestrationContext is null)
                throw new ArgumentNullException(nameof(orchestrationContext));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            orchestrationContext.SignalEntity(GetEntityId(command), CommandMetricEntity.ResetCount);
        }

        private static EntityId GetEntityId(ICommand command)
            => new EntityId(nameof(CommandMetricEntity), command.GetType().Name);
    }
}
