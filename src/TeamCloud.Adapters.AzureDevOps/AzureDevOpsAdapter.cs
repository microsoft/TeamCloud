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
using TeamCloud.Http;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Templates;

namespace TeamCloud.Adapters.AzureDevOps
{
    public sealed class AzureDevOpsAdapter : Adapter,
        IAdapterAuthorizable<DeploymentScope>
    {
        private const string VisualStudioAuthUrl = "https://app.vssps.visualstudio.com/oauth2/authorize";
        private const string VisualStudioTokenUrl = "https://app.vssps.visualstudio.com/oauth2/token";

        private readonly IFunctionsHost functionsHost;

        public AzureDevOpsAdapter(IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient, IFunctionsHost functionsHost = null)
            : base(sessionClient, tokenClient)
        {
            this.functionsHost = functionsHost ?? FunctionsHost.Default;
        }

        public bool CanHandle(DeploymentScope containerDocument)
            => containerDocument != null && containerDocument.Type == DeploymentScopeType.AzureDevOps;

        public AuthorizationSession CreateAuthorizationSession(DeploymentScope containerDocument)
        {
            if (containerDocument is null)
                throw new ArgumentNullException(nameof(containerDocument));

            if (!CanHandle(containerDocument))
                throw new ArgumentException("Argument value can not be handled", nameof(containerDocument));

            return new AzureDevOpsSession(Guid.Parse(containerDocument.Id))
            {
                TeamCloudOrganization = containerDocument.Organization,
                TeamCloudDeploymentScope = containerDocument.Id
            };
        }

        public Task<IActionResult> HandleAuthorizeAsync(HttpRequestMessage requestMessage, AuthorizationSession authorizationSession, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (authorizationSession is null)
                throw new ArgumentNullException(nameof(authorizationSession));

            if (authorizationEndpoints is null)
                throw new ArgumentNullException(nameof(authorizationEndpoints));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var azureDevOpsSession = (AzureDevOpsSession)authorizationSession;

            return requestMessage.Method switch
            {
                HttpMethod m when m == HttpMethod.Get => HandleAuthorizeGetAsync(requestMessage, azureDevOpsSession, authorizationEndpoints, log),
                HttpMethod m when m == HttpMethod.Post => HandleAuthorizePostAsync(requestMessage, azureDevOpsSession, authorizationEndpoints, log),
                _ => throw new NotImplementedException(),
            };
        }

        private async Task<IActionResult> HandleAuthorizeGetAsync(HttpRequestMessage requestMessage, AzureDevOpsSession authorizationSession, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            var queryParams = Url.ParseQueryParams(requestMessage.RequestUri.Query);
            var queryError = queryParams.GetValueOrDefault("error", StringComparison.OrdinalIgnoreCase);

            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "text/html",
                Content = Assembly.GetExecutingAssembly().GetManifestResourceTemplate($"{this.GetType().FullName}.html", new
                {
                    applicationWebsite = functionsHost.HostUrl,
                    applicationCallback = authorizationEndpoints.CallbackUrl,
                    session = authorizationSession,
                    error = queryError ?? string.Empty,
                    succeeded = queryParams.ContainsKey("succeeded")
                })
            };
        }

        private async Task<IActionResult> HandleAuthorizePostAsync(HttpRequestMessage requestMessage, AzureDevOpsSession authorizationSession, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
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
                .SetQueryParam("state", authorizationSession.SessionId)
                .SetQueryParam("scope", string.Join(' ', AzureDevOpsSession.Scopes))
                .SetQueryParam("redirect_uri", authorizationEndpoints.CallbackUrl)
                .ToString();

            return new RedirectResult(url);
        }

        public async Task<IActionResult> HandleCallbackAsync(HttpRequestMessage requestMessage, AuthorizationSession authorizationSession, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (authorizationSession is null)
                throw new ArgumentNullException(nameof(authorizationSession));

            if (authorizationEndpoints is null)
                throw new ArgumentNullException(nameof(authorizationEndpoints));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var queryParams = requestMessage.RequestUri.ParseQueryString();

            if (queryParams.TryGetValue("error", out string error))
            {
                return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", error));
            }
            else if (!queryParams.TryGetValue("state", out string state) || !authorizationSession.SessionId.Equals(state, StringComparison.OrdinalIgnoreCase))
            {
                return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", "Authorization state invalid"));
            }
            else
            {
                var form = new
                {
                    client_assertion_type = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                    client_assertion = (authorizationSession as AzureDevOpsSession).ClientSecret,
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
                    var azureDevOpsAuthorizationSession = (AzureDevOpsSession)authorizationSession;

                    var token = await responseMessage
                        .ReadAsJsonAsync<AzureDevOpsToken>()
                        .ConfigureAwait(false);

                    token.Organization = azureDevOpsAuthorizationSession.Organization;
                    token.ClientId = azureDevOpsAuthorizationSession.ClientId;
                    token.ClientSecret = azureDevOpsAuthorizationSession.ClientSecret;

                    _ = await TokenClient
                        .SetAsync(token)
                        .ConfigureAwait(false);
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
    }
}
