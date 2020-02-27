/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
        private readonly Lazy<AzureCredentials> credentials;
        private readonly Lazy<AZFluent.Azure.IAuthenticated> session;
        private readonly IAzureSessionOptions azureSessionOptions;
        private readonly IHttpClientFactory httpClientFactory;

        public AzureSessionService(IAzureSessionOptions azureSessionOptions = null, IHttpClientFactory httpClientFactory = null)
        {
            this.azureSessionOptions = azureSessionOptions ?? AzureSessionOptions.Default;
            this.httpClientFactory = httpClientFactory;

            credentials = new Lazy<AzureCredentials>(() =>
            {
                var credentialsFactory = new RMFluent.Authentication.AzureCredentialsFactory();

                if (string.IsNullOrEmpty(azureSessionOptions.ClientId))
                {
                    return credentialsFactory
                        .FromSystemAssignedManagedServiceIdentity(MSIResourceType.AppService, this.Environment, azureSessionOptions.TenantId);
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
            });

            session = new Lazy<AZFluent.Azure.IAuthenticated>(() =>
            {
                return AZFluent.Azure
                    .Configure()
                    .WithDelegatingHandler(this.httpClientFactory)
                    .Authenticate(credentials.Value);
            });
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
                var tokenProvider = new AzureServiceTokenProvider("RunAs=App;AppId=872cd9fa-d31f-45e0-9eab-6e460a02d1f1");

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
            var endpointUrl = AzureEnvironment.AzureGlobalCloud.GetEndpointUrl(azureEndpoint);

            return RestClient.Configure()
                .WithBaseUri(endpointUrl)
                .WithCredentials(credentials.Value)
                .WithDelegatingHandler(httpClientFactory)
                .Build();
        }

        public T CreateClient<T>(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint, Guid? subscriptionId = null)
            where T : FluentServiceClientBase<T>
        {
            var client = (T)Activator.CreateInstance(typeof(T), new object[] { CreateClient(azureEndpoint) });

            if (subscriptionId.HasValue
                && typeof(T).TryGetProperty("SubscriptionId", out PropertyInfo propertyInfo)
                && propertyInfo.PropertyType == typeof(string))
                propertyInfo.SetValue(client, subscriptionId.Value.ToString());

            return client;
        }
    }
}
