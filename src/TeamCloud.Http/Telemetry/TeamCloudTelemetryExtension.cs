/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Azure.WebJobs.Host.Config;

namespace TeamCloud.Http.Telemetry
{
    public sealed class TeamCloudTelemetryExtension : IExtensionConfigProvider
    {
        public static TelemetryConfiguration TelemetryConfiguration { get; private set; }

        private readonly TelemetryConfiguration telemetryConfiguration;
        private readonly IEnumerable<ITelemetryModule> telemetryModules;

        public TeamCloudTelemetryExtension(TelemetryConfiguration telemetryConfiguration, IEnumerable<ITelemetryModule> telemetryModules)
        {
            this.telemetryConfiguration = telemetryConfiguration ?? throw new ArgumentNullException(nameof(telemetryConfiguration));
            this.telemetryModules = telemetryModules ?? throw new ArgumentNullException(nameof(telemetryModules));
        }

        public void Initialize(ExtensionConfigContext context)
        {
            telemetryConfiguration
                .TelemetryProcessorChainBuilder
                .Use(next => new TeamCloudTelemetryProcessor(next))
                .Use(next => new QuickPulseTelemetryProcessor(next))
                .Build();

            ReInitQuickPulseProcessor();

            TelemetryConfiguration = telemetryConfiguration;
        }

        private void ReInitQuickPulseProcessor()
        {
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
    }
}
