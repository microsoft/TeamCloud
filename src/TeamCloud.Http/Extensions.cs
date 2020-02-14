/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Http
{

    public static class Extensions
    {
        public static IServiceCollection AddTeamCloudHttp(this IServiceCollection services, Action<GlobalFlurlHttpSettings> configure = null)
        {
            services.TryAddSingleton<IHttpClientFactory, TeamCloudHttpClientFactory>();

            FlurlHttp.Configure(configuration =>
            {
                configuration.HttpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
                configure?.Invoke(configuration);
            });

            return services;
        }

        public static Task<JObject> GetJObjectAsync(this IFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
            => request.GetJsonAsync(cancellationToken, completionOption).ContinueWith(task => (task.Result is null ? null : JObject.FromObject(task.Result)) as JObject, TaskContinuationOptions.OnlyOnRanToCompletion);

        public static Task<JObject> GetJObjectAsync(this Url url, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
            => url.GetJsonAsync(cancellationToken, completionOption).ContinueWith(task => (task.Result is null ? null : JObject.FromObject(task.Result)) as JObject, TaskContinuationOptions.OnlyOnRanToCompletion);

        public static Task<JObject> GetJObjectAsync(this string url, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
            => url.GetJsonAsync(cancellationToken, completionOption).ContinueWith(task => (task.Result is null ? null : JObject.FromObject(task.Result)) as JObject, TaskContinuationOptions.OnlyOnRanToCompletion);

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
            var match = Regex.Match(json.Trim(), @"^([{\[]).*([}\]])$", RegexOptions.Singleline);

            return match.Success && (new string[] { "{}", "[]" }).Contains($"{match.Groups[1].Value}{match.Groups[2].Value}");
        }
    }
}
