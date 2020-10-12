/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Middleware
{
    public class EnsureTeamCloudModelMiddleware : IMiddleware
    {
        private readonly ILoggerFactory loggerFactory;

        public EnsureTeamCloudModelMiddleware(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (next is null)
                throw new ArgumentNullException(nameof(next));

            var log = loggerFactory.CreateLogger(this.GetType());

            if (string.IsNullOrEmpty(ReferenceLink.BaseUrl))
            {
                ReferenceLink.BaseUrl = context.GetApplicationBaseUrl(true).AbsoluteUri;

                log.LogInformation($"Set TeamCloud.Model.Data.ReferenceLink BaseUrl to {ReferenceLink.BaseUrl}.");
            }

            await next(context).ConfigureAwait(false);
        }
    }
}
