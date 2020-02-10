/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using AZFluent = Microsoft.Azure.Management.Fluent;
using RMFluent = Microsoft.Azure.Management.ResourceManager.Fluent;
using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Flurl.Http.Configuration;
using System.Net.Http;

namespace TeamCloud.Azure
{
    public interface IAzureSessionService
    {
        IAzureSessionOptions Options { get; }

        AZFluent.Azure.IAuthenticated CreateSession();

        AZFluent.IAzure CreateSession(Guid subscriptionId);

        Task<string> AcquireTokenAsync(string authority);
    }

    public class AzureSessionService : IAzureSessionService
    {
        private readonly Lazy<AZFluent.Azure.IAuthenticated> session;
        private readonly IAzureSessionOptions azureSessionOptions;
        private readonly IHttpClientFactory httpClientFactory;

        public AzureSessionService(IAzureSessionOptions azureSessionOptions, IHttpClientFactory httpClientFactory = null)
        {
            this.azureSessionOptions = azureSessionOptions ?? throw new ArgumentNullException(nameof(azureSessionOptions));
            this.httpClientFactory = httpClientFactory;

            session = new Lazy<AZFluent.Azure.IAuthenticated>(() =>
            {
                var credentials = new RMFluent.Authentication.AzureCredentialsFactory()
                    .FromServicePrincipal(azureSessionOptions.ClientId, azureSessionOptions.ClientSecret, azureSessionOptions.TenantId, RMFluent.AzureEnvironment.AzureGlobalCloud);

                var configured = AZFluent.Azure
                    .Configure();

                var messageHandler = this.httpClientFactory?
                    .CreateMessageHandler() as DelegatingHandler;

                if (messageHandler != null)
                    configured = configured.WithDelegatingHandler(messageHandler);

                return configured
                    .Authenticate(credentials);
            });
        }

        public IAzureSessionOptions Options => azureSessionOptions;

        public async Task<string> AcquireTokenAsync(string authority)
        {
            var credentials = new ClientCredential(azureSessionOptions.ClientId, azureSessionOptions.ClientSecret);
            var context = new AuthenticationContext($"https://login.windows.net/{azureSessionOptions.TenantId}", true);
            var token = await context.AcquireTokenAsync(authority, credentials).ConfigureAwait(false);

            return token.AccessToken;
        }

        public AZFluent.Azure.IAuthenticated CreateSession()
            => session.Value;

        public AZFluent.IAzure CreateSession(Guid subscriptionId)
            => CreateSession().WithSubscription(subscriptionId.ToString());
    }
}
