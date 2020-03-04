/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using AZFluent = Microsoft.Azure.Management.Fluent;
using IHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;
using RMFluent = Microsoft.Azure.Management.ResourceManager.Fluent;

namespace TeamCloud.Azure
{
    public interface IAzureSessionService
    {
        AzureEnvironment Environment { get; }

        IAzureSessionOptions Options { get; }

        AZFluent.Azure.IAuthenticated CreateSession();

        AZFluent.IAzure CreateSession(Guid subscriptionId);

        Task<string> AcquireTokenAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint);

        Task<IAzureSessionIdentity> GetIdentityAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint);

        RestClient CreateClient(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint);

        T CreateClient<T>(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint, Guid? subscriptionId = null) where T : FluentServiceClientBase<T>;
    }

    public class AzureSessionService : IAzureSessionService
    {
        public static bool IsAzureEnvironment => !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

        private readonly Lazy<AzureCredentials> credentials;
        private readonly Lazy<AZFluent.Azure.IAuthenticated> session;
        private readonly IAzureSessionOptions azureSessionOptions;
        private readonly IHttpClientFactory httpClientFactory;

        public AzureSessionService(IAzureSessionOptions azureSessionOptions = null, IHttpClientFactory httpClientFactory = null)
        {
            this.azureSessionOptions = azureSessionOptions ?? AzureSessionOptions.Default;
            this.httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();

            credentials = new Lazy<AzureCredentials>(() => InitCredentials(), LazyThreadSafetyMode.PublicationOnly);
            session = new Lazy<AZFluent.Azure.IAuthenticated>(() => InitSession(), LazyThreadSafetyMode.PublicationOnly);
        }

        private AzureCredentials InitCredentials()
        {
            try
            {
                var credentialsFactory = new RMFluent.Authentication.AzureCredentialsFactory();

                if (string.IsNullOrEmpty(azureSessionOptions.ClientId))
                {
                    if (IsAzureEnvironment)
                    {
                        return credentialsFactory
                            .FromSystemAssignedManagedServiceIdentity(MSIResourceType.AppService, Environment, azureSessionOptions.TenantId);
                    }
                    else
                    {
                        return new AzureCredentials(
                            new TokenCredentials(new DevelopmentTokenProvider(this, AzureEndpoint.ResourceManagerEndpoint)),
                            new TokenCredentials(new DevelopmentTokenProvider(this, AzureEndpoint.GraphEndpoint)),
                            azureSessionOptions.TenantId,
                            Environment);
                    }
                }
                else if (string.IsNullOrEmpty(azureSessionOptions.ClientSecret))
                {
                    return credentialsFactory
                        .FromUserAssigedManagedServiceIdentity(azureSessionOptions.ClientId, MSIResourceType.AppService, this.Environment, azureSessionOptions.TenantId);
                }
                else
                {
                    return credentialsFactory
                        .FromServicePrincipal(azureSessionOptions.ClientId, azureSessionOptions.ClientSecret, azureSessionOptions.TenantId, this.Environment);
                }
            }
            catch (Exception exc)
            {
                throw new TypeInitializationException(typeof(AzureCredentials).FullName, exc);
            }
        }

        private AZFluent.Azure.IAuthenticated InitSession()
        {
            return AZFluent.Azure
                .Configure()
                .WithDelegatingHandler(this.httpClientFactory)
                .Authenticate(credentials.Value);
        }

        public AzureEnvironment Environment { get => AzureEnvironment.AzureGlobalCloud; }

        public IAzureSessionOptions Options { get => azureSessionOptions; }

        public async Task<IAzureSessionIdentity> GetIdentityAsync(AzureEndpoint azureEndpoint)
        {
            var token = await AcquireTokenAsync(azureEndpoint)
                .ConfigureAwait(false);

            var jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            var identity = new AzureSessionIdentity();

            if (jwtToken.Payload.TryGetValue("tid", out var tidValue) && Guid.TryParse(tidValue.ToString(), out Guid tid))
            {
                identity.TenantId = tid;
            }

            if (jwtToken.Payload.TryGetValue("oid", out var oidValue) && Guid.TryParse(oidValue.ToString(), out Guid oid))
            {
                identity.ObjectId = oid;
            }

            if (jwtToken.Payload.TryGetValue("appid", out var appidValue) && Guid.TryParse(appidValue.ToString(), out Guid appid))
            {
                identity.ClientId = appid;
            }

            return identity;
        }

        public Task<string> AcquireTokenAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint)
        {
            if (string.IsNullOrEmpty(azureSessionOptions.ClientId))
            {
                var tokenProvider = IsAzureEnvironment
                    ? new AzureServiceTokenProvider("RunAs=App")
                    : new AzureServiceTokenProvider("RunAs=Developer;DeveloperTool=AzureCLI");

                return tokenProvider.GetAccessTokenAsync(this.Environment.GetEndpointUrl(azureEndpoint));
            }
            else if (string.IsNullOrEmpty(azureSessionOptions.ClientSecret))
            {
                var tokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={azureSessionOptions.ClientId}");

                return tokenProvider.GetAccessTokenAsync(this.Environment.GetEndpointUrl(azureEndpoint));
            }
            else
            {
                var authenticationContext = new AuthenticationContext($"{this.Environment.AuthenticationEndpoint}{azureSessionOptions.TenantId}/", true);

                return authenticationContext
                    .AcquireTokenAsync(this.Environment.GetEndpointUrl(azureEndpoint), new ClientCredential(azureSessionOptions.ClientId, azureSessionOptions.ClientSecret))
                    .ContinueWith(task => task.Result.AccessToken, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        public AZFluent.Azure.IAuthenticated CreateSession()
            => session.Value;

        public AZFluent.IAzure CreateSession(Guid subscriptionId)
            => CreateSession().WithSubscription(subscriptionId.ToString());

        public RestClient CreateClient(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint)
        {
            try
            {
                var endpointUrl = AzureEnvironment.AzureGlobalCloud.GetEndpointUrl(azureEndpoint);

                return RestClient.Configure()
                    .WithBaseUri(endpointUrl)
                    .WithCredentials(credentials.Value)
                    .WithDelegatingHandler(httpClientFactory)
                    .Build();
            }
            catch (TypeInitializationException)
            {
                throw;
            }
            catch (Exception exc)
            {
                throw new TypeInitializationException(typeof(RestClient).FullName, exc);
            }
        }

        public T CreateClient<T>(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint, Guid? subscriptionId = null)
            where T : FluentServiceClientBase<T>
        {
            try
            {
                var client = (T)Activator.CreateInstance(typeof(T), new object[]
                {
                    CreateClient(azureEndpoint)
                });

                if (subscriptionId.HasValue
                    && typeof(T).TryGetProperty("SubscriptionId", out PropertyInfo propertyInfo)
                    && propertyInfo.PropertyType == typeof(string))
                    propertyInfo.SetValue(client, subscriptionId.Value.ToString());

                return client;
            }
            catch (TypeInitializationException)
            {
                throw;
            }
            catch (Exception exc)
            {
                throw new TypeInitializationException(typeof(T).FullName, exc);
            }
        }

        private class DevelopmentTokenProvider : ITokenProvider
        {
            private readonly IAzureSessionService azureSessionService;
            private readonly AzureEndpoint azureEndpoint;

            public DevelopmentTokenProvider(IAzureSessionService azureSessionService, AzureEndpoint azureEndpoint)
            {
                this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
                this.azureEndpoint = azureEndpoint;
            }

            public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
            {
                var token = await azureSessionService.AcquireTokenAsync(azureEndpoint).ConfigureAwait(false);

                return new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
