/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace TeamCloud.Http.Telemetry;

public class HttpTelemetryHandler : DelegatingHandler
{
    private readonly TelemetryConfiguration telemetryConfiguration;

    public HttpTelemetryHandler(TelemetryConfiguration telemetryConfiguration)
        => this.telemetryConfiguration = telemetryConfiguration ?? throw new ArgumentNullException(nameof(telemetryConfiguration));

    public HttpTelemetryHandler(HttpMessageHandler innerHandler, TelemetryConfiguration telemetryConfiguration = null) : base(innerHandler)
        => this.telemetryConfiguration = telemetryConfiguration ?? throw new ArgumentNullException(nameof(telemetryConfiguration));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var response = await base
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return await SendTelemetryAsync(request, response)
            .ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendTelemetryAsync(HttpRequestMessage request, HttpResponseMessage response)
    {
        const string headerPrefix = "x-ms-ratelimit";

        if (!string.IsNullOrWhiteSpace(telemetryConfiguration?.InstrumentationKey))
        {
            var telemetryClient = new TelemetryClient(telemetryConfiguration)
            {
                // usually there should be no need to set the instrumentation key
                // another time, as it is already part of the telemetry configuration.
                // however, there are some many different ways to inititalize
                // a new telemetry client that the approach we use here requires
                // this extra work.

                // for more information please check:
                // https://github.com/microsoft/ApplicationInsights-dotnet/issues/826

                InstrumentationKey = telemetryConfiguration.InstrumentationKey
            };

            var identity = Guid.Empty;

            if ((request.Headers.Authorization?.Scheme ?? string.Empty).Equals("bearer", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var jwtToken = new JwtSecurityTokenHandler()
                        .ReadJwtToken(request.Headers.Authorization.Parameter);

                    if (jwtToken.Payload.TryGetValue("oid", out var oidValue) && Guid.TryParse(oidValue.ToString(), out Guid oid))
                        identity = oid;
                }
                catch
                {
                    // swallow exceptions
                }
            }

            // the tracked rate limits are explained here:
            // https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/request-limits-and-throttling#remaining-requests

            await Task.WhenAll
            (
                TrackRateLimitAsync("remaining-subscription-reads", identity),
                TrackRateLimitAsync("remaining-subscription-writes", identity),
                TrackRateLimitAsync("remaining-tenant-reads", identity),
                TrackRateLimitAsync("remaining-tenant-writes", identity),
                TrackRateLimitAsync("remaining-subscription-resource-requests", identity),
                TrackRateLimitAsync("remaining-subscription-resource-entities-read", identity),
                TrackRateLimitAsync("remaining-tenant-resource-requests", identity),
                TrackRateLimitAsync("remaining-tenant-resource-entities-read", identity)
            )
            .ConfigureAwait(false);

            Task TrackRateLimitAsync(string headerName, Guid identity)
            {
                try
                {
                    var headerValue = response.GetHeaderValue($"{headerPrefix}-{headerName}");

                    if (double.TryParse(headerValue, out double rateLimit))
                    {
                        var metricName = Regex.Replace(headerName, "(^|-)([a-z])",
                            match => match.Value
                            .Replace("-", " ", StringComparison.OrdinalIgnoreCase)
                            .ToUpperInvariant());

                        telemetryClient
                            .GetMetric($"Azure Rate Limit {metricName}", "Identity")
                            .TrackValue(rateLimit, identity.ToString());
                    }
                }
                catch
                {
                    // swallow exceptions
                }

                return Task.CompletedTask;
            }
        }

        return response;
    }
}
