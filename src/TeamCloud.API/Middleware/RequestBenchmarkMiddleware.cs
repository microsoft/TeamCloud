using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TeamCloud.API.Middleware
{
    public class RequestBenchmarkMiddleware : IMiddleware
    {
        private static readonly TimeSpan benchmark = TimeSpan.FromSeconds(1);

        private readonly ILoggerFactory loggerFactory;

        public RequestBenchmarkMiddleware(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (next is null)
                throw new ArgumentNullException(nameof(next));

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next(context).ConfigureAwait(false);
            }
            finally
            {
                stopwatch.Stop();

                if (stopwatch.Elapsed > benchmark)
                {
                    loggerFactory
                        .CreateLogger(this.GetType())
                        .LogWarning($"{context.Request.Method} {context.Request.GetDisplayUrl()} took {stopwatch.ElapsedMilliseconds} msec and exceeds benchmark of {benchmark.TotalMilliseconds} msec !!!");
                }
            }
        }
    }
}
