/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Templates;

namespace TeamCloud.Adapters.GitHub
{
    public sealed class GitHubAdapter : Adapter, IAdapterAuthorize
    {
        public GitHubAdapter(IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient, IDistributedLockManager distributedLockManager)
            : base(sessionClient, tokenClient, distributedLockManager)
        { }

        public override DeploymentScopeType Type
            => DeploymentScopeType.GitHub;

        public override IEnumerable<ComponentType> ComponentTypes
            => new ComponentType[] { ComponentType.Repository };

        public override string DisplayName
            => base.DisplayName.Replace(" ", "");

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

        public override Task<Component> CreateComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log)
        {
            throw new NotImplementedException();
        }

        public override Task<Component> UpdateComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log)
        {
            throw new NotImplementedException();
        }

        public override Task<Component> DeleteComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log)
        {
            throw new NotImplementedException();
        }

        public override Task<NetworkCredential> GetServiceCredentialAsync(Component component)
        {
            throw new NotImplementedException();
        }
    }
}
