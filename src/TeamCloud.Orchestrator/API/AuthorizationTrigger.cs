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
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.API
{
    public sealed class AuthorizationTrigger
    {
        internal static string ResolveTokenValue(string token, DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                return default(string);

            var property = deploymentScope.GetType()
                .GetProperties()
                .SingleOrDefault(p => p.CanRead && p.Name.Equals(token, StringComparison.OrdinalIgnoreCase));

            var propertyValue = property?
                .GetValue(deploymentScope)?
                .ToString();

            return propertyValue is null
                ? null
                : HttpUtility.UrlEncode(propertyValue);
        }

        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IFunctionsHost functionsHost;
        private readonly IAdapter[] adapters;

        public AuthorizationTrigger(IDeploymentScopeRepository deploymentScopeRepository, IFunctionsHost functionsHost, IAdapter[] adapters)
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.functionsHost = functionsHost ?? FunctionsHost.Default;
            this.adapters = adapters ?? Array.Empty<IAdapter>();
        }

        private async Task<IActionResult> ExecuteAsync(string organization, string deploymentScopeId, Func<IAdapterAuthorize, DeploymentScope, IAuthorizationEndpoints, Task<IActionResult>> callback)
        {
            if (string.IsNullOrWhiteSpace(organization) || string.IsNullOrWhiteSpace(deploymentScopeId))
                return new NotFoundResult();

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(organization, deploymentScopeId)
                .ConfigureAwait(false);

            if (deploymentScope is null)
                return new NotFoundResult();

            var adapter = adapters
                .OfType<IAdapterAuthorize>()
                .FirstOrDefault(a => a.Type == deploymentScope.Type);

            if (adapter is null)
                return new NotFoundResult();

            var authorizationEndpoints = new AuthorizationEndpoints()
            {
                AuthorizationUrl = await FunctionsEnvironment
                    .GetFunctionUrlAsync(nameof(Authorize), functionsHost, replaceToken: (token) => ResolveTokenValue(token, deploymentScope))
                    .ConfigureAwait(false),

                CallbackUrl = await FunctionsEnvironment
                    .GetFunctionUrlAsync(nameof(Callback), functionsHost, replaceToken: (token) => ResolveTokenValue(token, deploymentScope))
                    .ConfigureAwait(false)
            };

            return await callback(adapter, deploymentScope, authorizationEndpoints)
                .ConfigureAwait(false);
        }

        [FunctionName(nameof(Authorize))]
        public Task<IActionResult> Authorize(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "authorize/{organization}/{id}")] HttpRequestMessage requestMessage,
            string organization,
            string id,
            ILogger log) => ExecuteAsync(organization, id, (authorizationAdapter, deploymentScope, authorizationEndpoints) =>
            {
                return authorizationAdapter.HandleAuthorizeAsync(deploymentScope, requestMessage, authorizationEndpoints, log);
            });

        [FunctionName(nameof(Callback))]
        public Task<IActionResult> Callback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "authorize/{organization}/{id}/callback")] HttpRequestMessage requestMessage,
            string organization,
            string id,
            ILogger log) => ExecuteAsync(organization, id, (authorizationAdapter, deploymentScope, authorizationEndpoints) =>
            {
                return authorizationAdapter.HandleCallbackAsync(deploymentScope, requestMessage, authorizationEndpoints, log);
            });

        private class AuthorizationEndpoints : IAuthorizationEndpoints
        {
            public string AuthorizationUrl { get; set; }

            public string CallbackUrl { get; set; }
        }
    }
}
