/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TeamCloud.Data;

namespace TeamCloud.API.Middleware
{
    public class EnsureTeamCloudConfigurationMiddleware : IMiddleware
    {
        private static bool Configured = false;

        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;

        public EnsureTeamCloudConfigurationMiddleware(ITeamCloudRepositoryReadOnly teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (next is null)
                throw new ArgumentNullException(nameof(next));

            // teamcloud needs a configuration in place to work properly.
            // to avoid calls that will fail because of a missing configuration
            // we will check its existance in this middleware and block
            // calls until a configuration is in place.

            // as we don't support to delete a configuration we can
            // keep the configured state once it was evaluated as true
            // to avoid unnecessary request to the configuration repository
            // for further requests.

            Configured = Configured || await teamCloudRepository.ExistsAsync().ConfigureAwait(false);

            if (Configured)
            {
                await next(context).ConfigureAwait(false);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await context.Response
                    .WriteAsync("Must POST a teamcloud.yaml file to api/config before calling any other APIs")
                    .ConfigureAwait(false);
            }
        }
    }
}
