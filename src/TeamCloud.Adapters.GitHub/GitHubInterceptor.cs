/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace TeamCloud.Adapters.GitHub
{
    internal sealed class GitHubInterceptor : IHttpClient
    {
        private readonly IHttpClient client;
        private readonly string acceptHeader;

        public GitHubInterceptor(IHttpClient client, string acceptHeader = null)
        {
            const string ACCEPT_HEADER_PREFIX = "application/vnd.github.";

            this.client = client ?? throw new ArgumentNullException(nameof(client));

            // do some accept header sanitization
            acceptHeader = acceptHeader?.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(acceptHeader) && !acceptHeader.StartsWith(ACCEPT_HEADER_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                acceptHeader = ACCEPT_HEADER_PREFIX + acceptHeader;

                if (acceptHeader.EndsWith("-preview", StringComparison.OrdinalIgnoreCase))
                    acceptHeader += "+json";
            }

            this.acceptHeader = acceptHeader;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<IResponse> Send(IRequest request, CancellationToken cancellationToken)
        {
            var requestDuration = Stopwatch.StartNew();
            var requestUrl = new Uri(request.BaseAddress, request.Endpoint).ToString();

            try
            {
                if (!string.IsNullOrWhiteSpace(acceptHeader))
                {
                    // override the api version information in
                    // the existing accept header for this request

                    request.Headers["Accept"] = acceptHeader;
                }

                Debug.WriteLine($"==> {request.Method.Method} {requestUrl} ");

                var response = await client
                    .Send(request, cancellationToken)
                    .ConfigureAwait(false);

                if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                    response.StatusCode != System.Net.HttpStatusCode.Accepted &&
                    response.Body is string body)
                {
                    var trace = new StringBuilder($"!!! {request.Method.Method} {requestUrl} {response.StatusCode}{Environment.NewLine}");

                    if (request.Headers.TryGetValue("Authorization", out var authorization))
                        trace.AppendLine($"AUTHORIZATION: {authorization}");

                    trace.AppendLine($"REQUEST:  {request.Body as string}");
                    trace.AppendLine($"RESPONSE: {response.Body as string}");

                    Debug.WriteLine(trace.ToString());
                }

                return response;
            }
            finally
            {
                Debug.WriteLine($"<== {request.Method.Method} {requestUrl} ({requestDuration.ElapsedMilliseconds} ms)");
            }
        }

        public void SetRequestTimeout(TimeSpan timeout)
        {
            client.SetRequestTimeout(timeout);
        }
    }
}
