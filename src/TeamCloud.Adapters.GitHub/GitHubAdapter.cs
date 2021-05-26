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
    public sealed class GitHubAdapter : Adapter, IAdapterAuthorize
    {
        public GitHubAdapter(IServiceProvider serviceProvider, IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient)
            : base(serviceProvider, sessionClient, tokenClient)
        { }

        public override DeploymentScopeType Type => DeploymentScopeType.GitHub;

        public override string DisplayName => base.DisplayName.Replace(" ", "");

        public override async Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var token = await TokenClient
                .GetAsync<GitHubToken>(deploymentScope)
                .ConfigureAwait(false);

            return !(token is null);
        }

        Task IAdapterAuthorize.CreateSessionAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (deploymentScope.Type != Type)
                throw new ArgumentException("Argument value can not be handled", nameof(deploymentScope));

            return SessionClient.SetAsync<GitHubSession>(new GitHubSession()
            {
                TeamCloudOrganization = deploymentScope.Organization,
                TeamCloudDeploymentScope = deploymentScope.Id
            });
        }

        Task<IActionResult> IAdapterAuthorize.HandleAuthorizeAsync(DeploymentScope deploymentScope, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
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

        Task<IActionResult> IAdapterAuthorize.HandleCallbackAsync(DeploymentScope deploymentScope, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
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

        public override Task<Component> CreateComponentAsync(Component component)
        {
            throw new NotImplementedException();
        }

        public override Task<Component> UpdateComponentAsync(Component component)
        {
            throw new NotImplementedException();
        }

        public override Task<Component> DeleteComponentAsync(Component component)
        {
            throw new NotImplementedException();
        }

        public override Task<NetworkCredential> GetServiceCredentialAsync(Component component)
        {
            throw new NotImplementedException();
        }
    }
}
