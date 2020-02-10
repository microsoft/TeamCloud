/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

namespace TeamCloud.Http
{
    public class TeamCloudHttpMessageHandler : DelegatingHandler
    {
        private readonly bool passthrough;

        public TeamCloudHttpMessageHandler(HttpMessageHandler innerHandler) : this(innerHandler, true)
        { }

        internal TeamCloudHttpMessageHandler(HttpMessageHandler innerHandler, bool passthrough) : base(innerHandler)
        {
            this.passthrough = passthrough;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;

            if (passthrough)
            {
                Debug.WriteLine($">>> {request.Method.ToString().ToUpperInvariant()} {request.RequestUri}");

                var responseTime = Stopwatch.StartNew();

                try
                {
                    response = await base
                        .SendAsync(request, cancellationToken)
                        .ConfigureAwait(false);
                }
                finally
                {
                    responseTime.Stop();
                }

                Debug.WriteLine($"<<< {request.Method.ToString().ToUpperInvariant()} {request.RequestUri} ({response.StatusCode} - {responseTime.Elapsed})");
            }
            else
            {
                Debug.WriteLine($"<=> {request.Method.ToString().ToUpperInvariant()} {request.RequestUri} (Flurl redirect)");

                response = await request.RequestUri.ToString()
                    .AllowAnyHttpStatus()
                    .WithHeaders(request.Headers)
                    .SendAsync(request.Method, request.Content, cancellationToken)
                    .ConfigureAwait(false);
            }

            return response;
        }
    }
}
