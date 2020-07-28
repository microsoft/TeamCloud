/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamCloud.Http.Telemetry;

namespace TeamCloud.Http
{

    public static class Extensions
    {
        public static IServiceCollection AddTeamCloudHttp(this IServiceCollection services, Action<GlobalFlurlHttpSettings> configure = null)
        {
            services.AddSingleton<ITelemetryInitializer>(new TeamCloudTelemetryInitializer(Assembly.GetCallingAssembly()));

            if (services.Any(sd => sd.ServiceType == typeof(IHttpClientFactory) && sd.ImplementationType == typeof(DefaultHttpClientFactory)))
            {
                services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory), CreateHttpClientFactory, ServiceLifetime.Singleton));
            }
            else
            {
                services.TryAddSingleton<IHttpClientFactory>(CreateHttpClientFactory);
            }

            FlurlHttp.Configure(configuration =>
            {
                configuration.HttpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

                configure?.Invoke(configuration);
            });

            return services;

            static IHttpClientFactory CreateHttpClientFactory(IServiceProvider serviceProvider)
            {
                try
                {
                    return ActivatorUtilities.CreateInstance<TeamCloudHttpClientFactory>(serviceProvider);
                }
                catch (InvalidOperationException)
                {
                    var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

                    var telemetryConfiguration = string.IsNullOrWhiteSpace(instrumentationKey)
                        ? new TelemetryConfiguration()
                        : new TelemetryConfiguration(instrumentationKey);

                    return new TeamCloudHttpClientFactory(telemetryConfiguration);
                }
            }
        }

        [SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Following the method syntax of Flurl")]
        public static Task<JObject> GetJObjectAsync(this IFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
            => request.GetJsonAsync<JObject>(cancellationToken, completionOption);

        [SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Following the method syntax of Flurl")]
        public static Task<JObject> GetJObjectAsync(this Url url, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
            => url.GetJsonAsync<JObject>(cancellationToken, completionOption);

        [SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Following the method syntax of Flurl")]
        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Following the method syntax of Flurl")]
        public static Task<JObject> GetJObjectAsync(this string url, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
            => url.GetJsonAsync<JObject>(cancellationToken, completionOption);

        public static T WithHeaders<T>(this T clientOrRequest, HttpHeaders headers)
            where T : IHttpSettingsContainer
        {
            if (headers == null)
                return clientOrRequest;

            foreach (var header in headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                clientOrRequest.WithHeader(header.Key, string.Join(' ', header.Value).Trim());

            return clientOrRequest;
        }

        public static bool IsJson(this string json)
        {
            if (json is null)
                throw new ArgumentNullException(nameof(json));

            var match = Regex.Match(json.Trim(), @"^([{\[]).*([}\]])$", RegexOptions.Singleline);

            return match.Success && (new string[] { "{}", "[]" }).Contains($"{match.Groups[1].Value}{match.Groups[2].Value}");
        }

        public static async Task<JObject> ReadAsJsonAsync(this HttpContent httpContent)
        {
            if (httpContent is null)
                throw new ArgumentNullException(nameof(httpContent));

            using var stream = await httpContent
                .ReadAsStreamAsync()
                .ConfigureAwait(false);

            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);

            return JObject.Load(jsonReader);
        }

        public static Task<T> ReadAsJsonAsync<T>(this HttpContent httpContent)
            => (httpContent ?? throw new ArgumentNullException(nameof(httpContent)))
            .ReadAsJsonAsync()
            .ContinueWith((json) => json.Result.ToObject<T>(), default, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

        public static Task<JObject> ReadAsJsonAsync(this HttpResponseMessage httpResponseMessage)
            => (httpResponseMessage ?? throw new ArgumentNullException(nameof(httpResponseMessage)))
            .Content.ReadAsJsonAsync();

        public static Task<T> ReadAsJsonAsync<T>(this HttpResponseMessage httpResponseMessage)
            => (httpResponseMessage ?? throw new ArgumentNullException(nameof(httpResponseMessage)))
            .Content.ReadAsJsonAsync<T>();

    }
}
