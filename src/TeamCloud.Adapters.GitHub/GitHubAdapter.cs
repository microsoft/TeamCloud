/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;
using TeamCloud.Templates;

namespace TeamCloud.Adapters.GitHub
{
    public sealed class GitHubAdapter : Adapter, IAdapterAuthorizable<DeploymentScope>
    {
        public GitHubAdapter(IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient)
            : base(sessionClient, tokenClient)
        { }

        public bool CanHandle(DeploymentScope containerDocument)
            => containerDocument != null && containerDocument.Type == DeploymentScopeType.GitHub;

        public AuthorizationSession CreateAuthorizationSession(DeploymentScope containerDocument)
        {
            if (!CanHandle(containerDocument))
                throw new ArgumentException("Argument value can not be handled", nameof(containerDocument));

            return new GitHubAuthorizationSession()
            {
                TeamCloudOrganization = containerDocument.Organization,
                TeamCloudDeploymentScope = containerDocument.Id
            };
        }

        public Task<IActionResult> HandleAuthorizeAsync(HttpRequestMessage requestMessage, AuthorizationSession authorizationSession, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            return Task.FromResult<IActionResult>(new ContentResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "text/html",
                Content = Assembly.GetExecutingAssembly().GetManifestResourceTemplate($"{this.GetType().FullName}_Authorize.html", new
                {
                })
            });
        }

        public Task<IActionResult> HandleCallbackAsync(HttpRequestMessage requestMessage, AuthorizationSession authorizationSession, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            return Task.FromResult<IActionResult>(new ContentResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "text/html",
                Content = Assembly.GetExecutingAssembly().GetManifestResourceTemplate($"{this.GetType().FullName}_Callback.html", new
                {
                })
            });
        }
    }
}
