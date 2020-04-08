/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

namespace TeamCloud.Http.Telemetry
{
    public sealed class TeamCloudTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor next;

        internal static void Register(TelemetryConfiguration telemetryConfiguration, IEnumerable<ITelemetryModule> telemetryModules)
        {
            telemetryConfiguration
                .TelemetryProcessorChainBuilder
                .Use(next => new TeamCloudTelemetryProcessor(next))
                .Use(next => new QuickPulseTelemetryProcessor(next))
                .Build();

            var quickPulseProcessor = telemetryConfiguration
                .TelemetryProcessors
                .OfType<QuickPulseTelemetryProcessor>()
                .LastOrDefault();

            if (quickPulseProcessor != null)
            {
                var quickPulseModule = telemetryModules
                    .OfType<QuickPulseTelemetryModule>()
                    .FirstOrDefault();

                quickPulseModule?.RegisterTelemetryProcessor(quickPulseProcessor);
            }

            telemetryConfiguration
                .TelemetryProcessors
                .OfType<ITelemetryModule>()
                .ToList()
                .ForEach(module => module.Initialize(telemetryConfiguration));
        }

        internal TeamCloudTelemetryProcessor(ITelemetryProcessor next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public void Process(ITelemetry item)
        {
            if (item is DependencyTelemetry dependencyTelemetry && !ShouldForward(dependencyTelemetry))
                return;

            this.next.Process(item);
        }

        private static bool ShouldForward(DependencyTelemetry dependencyTelemetry)
        {
            var forward = true;

            if (dependencyTelemetry.Type?.Equals("azure table", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                forward = dependencyTelemetry.ResultCode switch
                {
                    "404" => false, // not found - false positive
                    "409" => false, // conflict - usually caused by "create if not exist" operations
                    _ => forward
                };
            }

            return forward;
        }
    }
}
