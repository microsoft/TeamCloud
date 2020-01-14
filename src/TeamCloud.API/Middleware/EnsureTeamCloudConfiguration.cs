/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TeamCloud.Data;
using TeamCloud.Model.Data;

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
            if (Configured)
            {
                await next(context);
            }
            else if (context.Request.Path.StartsWithSegments("/api/config", StringComparison.OrdinalIgnoreCase)
                  && context.Request.Method == HttpMethods.Post)
            {
                await next(context);
            }
            else
            {
                var teamCloud = await teamCloudRepository.GetAsync().ConfigureAwait(false);

                if (teamCloud?.Configuration != null)
                {
                    var teamCloudConfigValidation = await new TeamCloudConfigurationValidator().ValidateAsync(teamCloud.Configuration);

                    if (teamCloudConfigValidation.IsValid)
                    {
                        Configured = true;

                        await next(context);

                        return;
                    }
                }

                // not configured

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await context.Response.WriteAsync("Must POST a teamcloud.yaml file to api/config before calling any other APIs");
            }
        }
    }
}
