/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Entities
{
    public class CommandMetricEntity
    {
        internal const string IncrementCount = nameof(IncrementCount);
        internal const string DecrementCount = nameof(DecrementCount);
        internal const string ResetCount = nameof(ResetCount);

        private readonly TelemetryClient telemetryClient;

        public CommandMetricEntity(TelemetryConfiguration telemetryConfiguration = null)
        {
            if (string.IsNullOrEmpty(telemetryConfiguration?.InstrumentationKey))
            {
                // not telemetry configuration available or no instrumentation key given
                // no need to initialize a telemetry client to track metrics

                telemetryClient = null;
            }
            else
            {
                telemetryClient = new TelemetryClient(telemetryConfiguration)
                {
                    InstrumentationKey = telemetryConfiguration.InstrumentationKey
                };
            }
        }

        [FunctionName(nameof(CommandMetricEntity))]
        public void Run(
            [EntityTrigger] IDurableEntityContext entityContext,
            ILogger log)
        {
            if (entityContext is null)
                throw new ArgumentNullException(nameof(entityContext));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var count = entityContext.GetState<uint>(() => 0);

            try
            {
                switch (entityContext.OperationName)
                {
                    case IncrementCount:
                        count++;
                        break;

                    case DecrementCount:
                        count--;
                        break;

                    case ResetCount:
                        count = 0;
                        break;

                    default:
                        throw new NotSupportedException($"Operation '{entityContext.OperationName}' is not supported by entity '{nameof(CommandMetricEntity)}'");
                }

                entityContext.SetState(count);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Operation '{entityContext.OperationName}' failed on entity '{nameof(CommandMetricEntity)}'");
            }
            finally
            {
                telemetryClient?
                    .GetMetric("Command Rates", "Command Type")
                    .TrackValue(count, entityContext.EntityKey);
            }
        }
    }
}
