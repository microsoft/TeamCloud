/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;
using TeamCloud.Templates;

namespace TeamCloud.Adapters.AzureDevOps
{
    public sealed class AzureDevOpsAdapter : Adapter, IAdapterAuthorize
    {
        private const string VisualStudioAuthUrl = "https://app.vssps.visualstudio.com/oauth2/authorize";
        private const string VisualStudioTokenUrl = "https://app.vssps.visualstudio.com/oauth2/token";

        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IFunctionsHost functionsHost;

        public AzureDevOpsAdapter(IServiceProvider serviceProvider, IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient, IDeploymentScopeRepository deploymentScopeRepository, IFunctionsHost functionsHost = null)
            : base(serviceProvider, sessionClient, tokenClient)
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.functionsHost = functionsHost ?? FunctionsHost.Default;
        }

        public override DeploymentScopeType Type => DeploymentScopeType.AzureDevOps;

        public override string DisplayName => "Azure DevOps";

        public override async Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var token = await TokenClient
                .GetAsync<AzureDevOpsToken>(deploymentScope)
                .ConfigureAwait(false);

            return !(token is null);
        }

        Task IAdapterAuthorize.CreateSessionAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (deploymentScope.Type != Type)
                throw new ArgumentException("Argument value can not be handled", nameof(deploymentScope));

            return SessionClient.SetAsync(new AzureDevOpsSession(deploymentScope));
        }

        async Task<IActionResult> IAdapterAuthorize.HandleAuthorizeAsync(DeploymentScope deploymentScope, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (authorizationEndpoints is null)
                throw new ArgumentNullException(nameof(authorizationEndpoints));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var authorizationSession = await SessionClient
                .GetAsync<AzureDevOpsSession>(deploymentScope)
                .ConfigureAwait(false);

            var task = requestMessage.Method switch
            {
                HttpMethod m when m == HttpMethod.Get => HandleAuthorizeGetAsync(authorizationSession, requestMessage, authorizationEndpoints, log),
                HttpMethod m when m == HttpMethod.Post => HandleAuthorizePostAsync(authorizationSession, requestMessage, authorizationEndpoints, log),
                _ => Task.FromException<IActionResult>(new NotImplementedException())
            };

            return await task.ConfigureAwait(false);
        }

        async Task<IActionResult> IAdapterAuthorize.HandleCallbackAsync(DeploymentScope deploymentScope, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (authorizationEndpoints is null)
                throw new ArgumentNullException(nameof(authorizationEndpoints));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var authorizationSession = await SessionClient
                .GetAsync<AzureDevOpsSession>(deploymentScope)
                .ConfigureAwait(false);

            if (authorizationSession is null)
            {
                return new NotFoundResult();
            }

            var queryParams = requestMessage.RequestUri.ParseQueryString();

            if (queryParams.TryGetValue("error", out string error))
            {
                return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", error));
            }
            else if (!queryParams.TryGetValue("state", out string state) || !authorizationSession.SessionState.Equals(state, StringComparison.OrdinalIgnoreCase))
            {
                return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", "Authorization state invalid"));
            }
            else
            {
                var form = new
                {
                    client_assertion_type = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                    client_assertion = authorizationSession.ClientSecret,
                    grant_type = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    assertion = queryParams.Get("code"),
                    redirect_uri = authorizationEndpoints.CallbackUrl
                };

                var responseMessage = await VisualStudioTokenUrl
                    .WithHeaders(new MediaTypeWithQualityHeaderValue("application/json"))
                    .AllowAnyHttpStatus()
                    .PostUrlEncodedAsync(form)
                    .ConfigureAwait(false);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var azureDevOpsAuthorizationToken = new AzureDevOpsToken(deploymentScope);

                    azureDevOpsAuthorizationToken.Organization = authorizationSession.Organization;
                    azureDevOpsAuthorizationToken.ClientId = authorizationSession.ClientId;
                    azureDevOpsAuthorizationToken.ClientSecret = authorizationSession.ClientSecret;

                    var json = await responseMessage
                        .ReadAsJsonAsync()
                        .ConfigureAwait(false);

                    TeamCloudSerialize.PopulateObject(json.ToString(), azureDevOpsAuthorizationToken);

                    _ = await TokenClient
                        .SetAsync(azureDevOpsAuthorizationToken)
                        .ConfigureAwait(false);

                    log.LogInformation($"Token information successfully acquired.");
                }
                else
                {
                    error = await (responseMessage.StatusCode == HttpStatusCode.BadRequest
                        ? GetErrorDescriptionAsync(responseMessage)
                        : Task.FromResult(responseMessage.ReasonPhrase)).ConfigureAwait(false);

                    return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", error));
                }
            }

            _ = await SessionClient
                .SetAsync(authorizationSession)
                .ConfigureAwait(false);

            return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("succeeded"));

            async Task<string> GetErrorDescriptionAsync(HttpResponseMessage responseMessage)
            {
                try
                {
                    var json = await responseMessage
                        .ReadAsJsonAsync()
                        .ConfigureAwait(false);

                    return json?.SelectToken("$..ErrorDescription")?.ToString() ?? json.ToString();
                }
                catch
                {
                    return null;
                }
            }
        }

        private async Task<IActionResult> HandleAuthorizeGetAsync(AzureDevOpsSession authorizationSession, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            var queryParams = Url.ParseQueryParams(requestMessage.RequestUri.Query);
            var queryError = queryParams.GetValueOrDefault("error", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(queryError))
            {
                log.LogWarning($"Authorization failed: {queryError}");
            }
            else if (queryParams.ContainsKey("succeeded"))
            {
                log.LogInformation($"Authorization succeeded");
            }

            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "text/html",
                Content = Assembly.GetExecutingAssembly().GetManifestResourceTemplate($"{GetType().FullName}.html", new
                {
                    applicationWebsite = functionsHost.HostUrl,
                    applicationCallback = authorizationEndpoints.CallbackUrl,
                    session = authorizationSession,
                    error = queryError ?? string.Empty,
                    succeeded = queryParams.ContainsKey("succeeded")
                })
            };
        }

        private async Task<IActionResult> HandleAuthorizePostAsync(AzureDevOpsSession authorizationSession, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            var payload = await requestMessage.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            var payloadParams = Url.ParseQueryParams(payload);

            authorizationSession.Organization = payloadParams.GetValueOrDefault("organization", StringComparison.OrdinalIgnoreCase);
            authorizationSession.ClientId = payloadParams.GetValueOrDefault("client_id", StringComparison.OrdinalIgnoreCase);
            authorizationSession.ClientSecret = payloadParams.GetValueOrDefault("client_secret", StringComparison.OrdinalIgnoreCase);
            authorizationSession = await SessionClient.SetAsync(authorizationSession).ConfigureAwait(false);

            var url = VisualStudioAuthUrl
                .SetQueryParam("client_id", authorizationSession.ClientId)
                .SetQueryParam("response_type", "Assertion")
                .SetQueryParam("state", authorizationSession.SessionState)
                .SetQueryParam("scope", string.Join(' ', AzureDevOpsSession.Scopes))
                .SetQueryParam("redirect_uri", authorizationEndpoints.CallbackUrl)
                .ToString();

            log.LogDebug($"Redirecting authorize POST response to {url}");

            return new RedirectResult(url);
        }

    }
}
