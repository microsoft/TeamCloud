/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using TeamCloud.Http.Telemetry;

namespace TeamCloud.Http;

public class TeamCloudHttpClientFactory : DefaultHttpClientFactory
{
    private readonly TelemetryConfiguration telemetryConfiguration;

    public TeamCloudHttpClientFactory(TelemetryConfiguration telemetryConfiguration = null)
        => this.telemetryConfiguration = telemetryConfiguration ?? new TelemetryConfiguration(Guid.Empty.ToString());

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Lifetime is managed by the returned HttpMessageHandler instance.")]
    public override HttpMessageHandler CreateMessageHandler()
    {
        var innerHandler = new HttpTelemetryHandler(base.CreateMessageHandler(), telemetryConfiguration);
        var passthrough = Assembly.GetCallingAssembly().Equals(typeof(FlurlClient).Assembly);

        return new TeamCloudHttpMessageHandler(innerHandler, passthrough);
    }
}
