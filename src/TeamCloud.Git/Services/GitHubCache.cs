using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using Octokit.Internal;
using TeamCloud.Git.Caching;

namespace TeamCloud.Git.Services
{
    internal sealed class GitHubCache : IHttpClient
    {
        private readonly IHttpClient client;
        private readonly IRepositoryCache cache;

        public GitHubCache(IHttpClient client, IRepositoryCache cache)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        public async Task<IResponse> Send(IRequest request, CancellationToken cancellationToken)
        {

            if (request.Method != HttpMethod.Get)
            {
                return await client
                    .Send(request, cancellationToken)
                    .ConfigureAwait(false);
            }

            var cacheKey = new Uri(request.BaseAddress, request.Endpoint).ToString();

            var response = await ReadCacheAsync(cacheKey, cancellationToken)
                .ConfigureAwait(false);

            if (response is null)
            {
                response = await client
                    .Send(request, cancellationToken)
                    .ConfigureAwait(false);

                return await WriteCacheAsync(cacheKey, response, cancellationToken)
                    .ConfigureAwait(false);
            }

            // add a conditional request header so our 
            // request won't affect the GitHub rate limit.

            request.Headers["If-None-Match"] = response.ApiInfo.Etag;

            var conditionalResponse = await client
                .Send(request, cancellationToken)
                .ConfigureAwait(false);

            if (conditionalResponse.StatusCode == HttpStatusCode.NotModified)
            {
                // the conditional response indicates that our cached response 
                // is still up-to-date - we are done and return the cached response

                return response;
            }
            else
            {
                // the conditional response indicates that our cached response is out of date.
                // update the cache using the conditional response and return the response.

                return await WriteCacheAsync(cacheKey, conditionalResponse, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private async Task<IResponse> WriteCacheAsync(string endpoint, IResponse response, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(response?.ApiInfo?.Etag))
            {
                var data = JsonConvert.SerializeObject(response, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None });

                await cache
                    .SetAsync(endpoint, data, cancellationToken)
                    .ConfigureAwait(false);

                const string RateLimitHeader = "x-ratelimit-";

                var rateLimitData = response.Headers
                    .Where(kvp => kvp.Key.StartsWith(RateLimitHeader, StringComparison.OrdinalIgnoreCase))
                    .Select(kvp => new KeyValuePair<string, string>(kvp.Key.Substring(RateLimitHeader.Length), kvp.Value));

                // TODO: write some telemetry data to AppInsight instead of using debug output as telemetry sink
                Debug.WriteLine($"{nameof(GitHubCache)} - { string.Join(" / ", rateLimitData.Select(kvp => $"{kvp.Key}={kvp.Value}")) }");
            }

            return response;
        }

        private async Task<IResponse> ReadCacheAsync(string endpoint, CancellationToken cancellationToken)
        {
            var data = await cache
                .GetAsync(endpoint, cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(data))
                return null;

            return JsonConvert.DeserializeObject<CachedResponse>(data);
        }

        public void SetRequestTimeout(TimeSpan timeout)
            => client.SetRequestTimeout(timeout);

        internal sealed class CachedResponse : IResponse
        {
            public object Body { get; set; }

            public IReadOnlyDictionary<string, string> Headers { get; set; }

            public ApiInfo ApiInfo { get; set; }

            public HttpStatusCode StatusCode { get; set; }

            public string ContentType { get; set; }
        }
    }
}
