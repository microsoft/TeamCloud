/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.API
{
    public sealed class AuthorizationTrigger
    {
        internal static string ResolveTokenValue(string token, AuthorizationSession authorizationSession)
        {
            var property = authorizationSession.GetType()
                .GetProperties()
                .SingleOrDefault(p => p.CanRead && p.Name.Equals(token, StringComparison.OrdinalIgnoreCase));

            var propertyValue = property?.GetValue(authorizationSession)?.ToString();
            return propertyValue is null ? null : HttpUtility.UrlEncode(propertyValue);
        }

        private readonly IAuthorizationSessionClient authorizationSessionClient;
        private readonly IFunctionsHost functionsHost;
        private readonly IAdapter[] adapters;

        public AuthorizationTrigger(IAuthorizationSessionClient authorizationSessionClient, IFunctionsHost functionsHost, IAdapter[] adapters)
        {
            this.authorizationSessionClient = authorizationSessionClient ?? throw new ArgumentNullException(nameof(authorizationSessionClient));
            this.functionsHost = functionsHost ?? FunctionsHost.Default;
            this.adapters = adapters ?? Array.Empty<IAdapter>();
        }

        private async Task<IActionResult> ExecuteAsync(string authId, Func<IAdapterAuthorizable, AuthorizationSession, IAuthorizationEndpoints, Task<IActionResult>> callback)
        {
            if (string.IsNullOrWhiteSpace(authId))
                return new NotFoundResult();

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var authorizationSession = await authorizationSessionClient
                .GetAsync(authId)
                .ConfigureAwait(false);

            if (authorizationSession is null)
                return new NotFoundResult();

            var adapter = adapters
                .OfType<IAdapterAuthorizable>()
                .FirstOrDefault(a => a.GetType().Equals(authorizationSession.Adapter));

            if (adapter is null)
                return new NotFoundResult();

            var authorizationEndpoints = new AuthorizationEndpoints()
            {
                AuthorizationUrl = await FunctionsEnvironment
                    .GetFunctionUrlAsync(nameof(Authorize), functionsHost, replaceToken: (token) => ResolveTokenValue(token, authorizationSession))
                    .ConfigureAwait(false),

                CallbackUrl = await FunctionsEnvironment
                    .GetFunctionUrlAsync(nameof(Callback), functionsHost, replaceToken: (token) => ResolveTokenValue(token, authorizationSession))
                    .ConfigureAwait(false)
            };

            return await callback(adapter, authorizationSession, authorizationEndpoints)
                .ConfigureAwait(false);
        }

        [FunctionName(nameof(Authorize))]
        public Task<IActionResult> Authorize(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "authorize/{authId}")] HttpRequestMessage requestMessage,
            string authId,
            ILogger log) => ExecuteAsync(authId, (authorizationAdapter, authorizationSession, authorizationEndpoints) =>
            {
                return authorizationAdapter.HandleAuthorizeAsync(requestMessage, authorizationSession, authorizationEndpoints, log);
            });

        [FunctionName(nameof(Callback))]
        public Task<IActionResult> Callback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "authorize/{authId}/callback")] HttpRequestMessage requestMessage,
            string authId,
            ILogger log) => ExecuteAsync(authId, (authorizationAdapter, authorizationSession, authorizationEndpoints) =>
            {
                return authorizationAdapter.HandleCallbackAsync(requestMessage, authorizationSession, authorizationEndpoints, log);
            });

        private class AuthorizationEndpoints : IAuthorizationEndpoints
        {
            public string AuthorizationUrl { get; set; }

            public string CallbackUrl { get; set; }
        }
    }
}
