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

namespace TeamCloud.Http
{

    public static class Extensions
    {
        public static IServiceCollection AddTeamCloudHttp(this IServiceCollection services, Action<GlobalFlurlHttpSettings> configure = null)
        {
            if (services.Any(sd => sd.ServiceType == typeof(IHttpClientFactory) && sd.ImplementationType == typeof(DefaultHttpClientFactory)))
            {
                services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory), HttpClientFactoryInitializer, ServiceLifetime.Singleton));
            }
            else
            {
                services.TryAddSingleton<IHttpClientFactory>(HttpClientFactoryInitializer);
            }

            FlurlHttp.Configure(configuration =>
            {
                configuration.HttpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

                configure?.Invoke(configuration);
            });

            return services;

            static IHttpClientFactory HttpClientFactoryInitializer(IServiceProvider serviceProvider)
            {
                try
                {
                    return (IHttpClientFactory)ActivatorUtilities.CreateInstance<TeamCloudHttpClientFactory>(serviceProvider);
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
            => (request ?? throw new ArgumentNullException(nameof(request)))
            .GetJsonAsync(cancellationToken, completionOption)
            .ContinueWith(task => (task.Result is null ? null : JObject.FromObject(task.Result)) as JObject, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

        [SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Following the method syntax of Flurl")]
        public static Task<JObject> GetJObjectAsync(this Url url, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
            => (url ?? throw new ArgumentNullException(nameof(url)))
            .GetJsonAsync(cancellationToken, completionOption)
            .ContinueWith(task => (task.Result is null ? null : JObject.FromObject(task.Result)) as JObject, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

        [SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Following the method syntax of Flurl")]
        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Following the method syntax of Flurl")]
        public static Task<JObject> GetJObjectAsync(this string url, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
            => (url ?? throw new ArgumentNullException(nameof(url)))
            .GetJsonAsync(cancellationToken, completionOption)
            .ContinueWith(task => (task.Result is null ? null : JObject.FromObject(task.Result)) as JObject, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

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
