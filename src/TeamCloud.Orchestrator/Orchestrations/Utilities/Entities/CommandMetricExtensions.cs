/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Entities
{
    public static class CommandMetricExtensions
    {
        internal static IDisposable TrackCommandMetrics(this IDurableOrchestrationContext functionContext, ICommand command)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            functionContext.SignalEntity(GetEntityId(command), CommandMetricEntity.IncrementCount);

            return new CommandMetricScope(() => functionContext.SignalEntity(GetEntityId(command), CommandMetricEntity.DecrementCount));
        }

        internal static void ResetCommandMetrics(this IDurableOrchestrationContext functionContext, ICommand command)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            functionContext.SignalEntity(GetEntityId(command), CommandMetricEntity.ResetCount);
        }

        private static EntityId GetEntityId(ICommand command)
            => new EntityId(nameof(CommandMetricEntity), command.GetType().Name);
    }
}
