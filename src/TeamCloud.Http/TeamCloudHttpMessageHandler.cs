/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
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

                Debug.WriteLine($"<<< {request.Method.ToString().ToUpperInvariant()} {request.RequestUri} {response.StatusCode} ({responseTime.ElapsedMilliseconds} msec)");

#if DEBUG
                if (!response.IsSuccessStatusCode)
                    await TraceErrorAsync(request, response);
#endif
            }
            else
            {
                Debug.WriteLine($"<=> {request.Method.ToString().ToUpperInvariant()} {request.RequestUri}");

                response = await request.RequestUri.ToString()
                    .AllowAnyHttpStatus()
                    .WithHeaders(request.Headers)
                    .SendAsync(request.Method, request.Content, cancellationToken)
                    .ConfigureAwait(false);
            }

            return response;
        }

        private async Task TraceErrorAsync(HttpRequestMessage request, HttpResponseMessage response)
        {
            await response.Content
                .LoadIntoBufferAsync()
                .ConfigureAwait(false);

            var trace = new StringBuilder();

            trace.AppendLine("REQUEST:  " + await request.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false));

            trace.AppendLine("RESPONSE: " + await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false));

            Debug.WriteLine($"!!! {request.Method.ToString().ToUpperInvariant()} {request.RequestUri} {response.StatusCode}{Environment.NewLine}{trace}");
        }
    }
}
