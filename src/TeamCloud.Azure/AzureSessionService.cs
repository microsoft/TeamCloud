/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Flurl.Http.Configuration;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using AZFluent = Microsoft.Azure.Management.Fluent;
using IHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;
using RMFluent = Microsoft.Azure.Management.ResourceManager.Fluent;

namespace TeamCloud.Azure;

public class AzureSessionService : IAzureSessionService
{
    public static bool IsAzureEnvironment =>
        !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

    public static Task<string> AcquireTokenAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint, IAzureSessionOptions azureSessionOptions = null, IHttpClientFactory httpClientFactory = null)
        => new AzureSessionService(azureSessionOptions, httpClientFactory).AcquireTokenAsync(azureEndpoint);

    private readonly IAzureSessionOptions azureSessionOptions;
    private readonly IHttpClientFactory httpClientFactory;

    public AzureSessionService(IAzureSessionOptions azureSessionOptions = null, IHttpClientFactory httpClientFactory = null)
    {
        this.azureSessionOptions = azureSessionOptions ?? AzureSessionOptions.Default;
        this.httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();
    }

    private async Task<AzureCredentials> GetCredentialsAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(azureSessionOptions.TenantId) && azureSessionOptions == AzureSessionOptions.Default)
            {
                var identity = await GetIdentityAsync(AzureEndpoint.ResourceManagerEndpoint)
                    .ConfigureAwait(false);

                if ((identity?.TenantId).HasValue)
                    ((AzureSessionOptions)azureSessionOptions).TenantId = identity.TenantId.ToString();
            }

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

    public AzureEnvironment Environment { get => AzureEnvironment.AzureGlobalCloud; }

    public IAzureSessionOptions Options { get => azureSessionOptions; }

    public async Task<IAzureSessionIdentity> GetIdentityAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint)
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

    public async Task<Guid> GetTenantIdAsync()
    {
        if (Guid.TryParse(azureSessionOptions.TenantId, out var tenantId))
        {
            return tenantId;
        }
        else
        {
            var identity = await GetIdentityAsync()
                .ConfigureAwait(false);

            return identity.TenantId;
        }
    }

    public async Task<string> AcquireTokenAsync(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint)
    {
        AccessToken accessToken;

        if (string.IsNullOrEmpty(azureSessionOptions.ClientId))
        {
            if (IsAzureEnvironment)
            {
                var managedIdentityCredential = new ManagedIdentityCredential();

                accessToken = await managedIdentityCredential.GetTokenAsync(
                    new TokenRequestContext(scopes: new string[] { this.Environment.GetEndpointUrl(azureEndpoint) + "/.default" }) { }
                ).ConfigureAwait(false);
            }
            else
            {
                // ensure we disable SSL verfication for this process when using the Azure CLI to aqcuire MSI token.
                // otherwise our code will fail in dev scenarios where a dev proxy like fiddler is running to sniff
                // http traffix between our services or between service and other reset apis (e.g. Azure)
                System.Environment.SetEnvironmentVariable("AZURE_CLI_DISABLE_CONNECTION_VERIFICATION", "1", EnvironmentVariableTarget.Process);

                var azureCliCredential = new AzureCliCredential();

                accessToken = await azureCliCredential.GetTokenAsync(
                    new TokenRequestContext(scopes: new string[] { this.Environment.GetEndpointUrl(azureEndpoint) + "/.default" }) { }
                ).ConfigureAwait(false);
            }

            return accessToken.Token;
        }
        else if (string.IsNullOrEmpty(azureSessionOptions.ClientSecret))
        {
            var managedIdentityCredential = new ManagedIdentityCredential(azureSessionOptions.ClientId);

            accessToken = await managedIdentityCredential.GetTokenAsync(
                new TokenRequestContext(scopes: new string[] { this.Environment.GetEndpointUrl(azureEndpoint) + "/.default" }) { }
            ).ConfigureAwait(false);

            return accessToken.Token;
        }
        else
        {
            var app = ConfidentialClientApplicationBuilder.Create(azureSessionOptions.ClientId)
                .WithClientSecret(azureSessionOptions.ClientSecret)
                .WithAuthority($"{this.Environment.AuthenticationEndpoint}{azureSessionOptions.TenantId}/", true)
                .Build();

            var authResult = await app.AcquireTokenForClient(new[] { $"{this.Environment.GetEndpointUrl(azureEndpoint)}/.default" })
                // .WithTenantId(specificTenant)
                // See https://aka.ms/msal.net/withTenantId
                .ExecuteAsync()
                .ConfigureAwait(false);

            return authResult.AccessToken;
        }
    }

    public async Task<AZFluent.Azure.IAuthenticated> CreateSessionAsync()
    {
        var credentials = await GetCredentialsAsync()
            .ConfigureAwait(false);

        return AZFluent.Azure
              .Configure()
              .WithDelegatingHandler(this.httpClientFactory)
              .Authenticate(credentials);
    }

    public async Task<AZFluent.IAzure> CreateSessionAsync(Guid subscriptionId)
    {
        var session = await CreateSessionAsync()
            .ConfigureAwait(false);

        return session.WithSubscription(subscriptionId.ToString());
    }

    public async Task<T> CreateClientAsync<T>(AzureEndpoint azureEndpoint = AzureEndpoint.ResourceManagerEndpoint, Guid? subscriptionId = default)
        where T : ServiceClient<T>
    {
        static bool CanCreateWith(params Type[] parameterTypes) => typeof(T).GetConstructors().Any(constructor =>
        {
            var parameters = constructor.GetParameters();

            if (parameters.Length < parameterTypes.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameters[i].ParameterType != parameterTypes[i]
                    && !parameters[i].ParameterType.IsAssignableFrom(parameterTypes[i]))
                    {
                        if (i == (parameters.Length - 1) && parameters[i].GetCustomAttribute<ParamArrayAttribute>() is not null)
                        {
                            // edge case - the last parameter of the constructor can also
                            // be an "params" parameter. means the parametertype is a array

                            if (parameters[i].ParameterType.GetElementType() != parameterTypes[i]
                    && !parameters[i].ParameterType.GetElementType().IsAssignableFrom(parameterTypes[i]))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            // the provided parameter type don't match the constructor's parameter
                            // type at the same position and is also not assignable

                            return false;
                        }
                    }
                }

                if (parameters.Length > parameterTypes.Length
                && !parameters.Skip(parameterTypes.Length).All(parameter => parameter.IsOptional))
                {
                    // the constructor's parameter list is longer than the provided set of parameter
                    // types, but at least on of the constructor's paramters is not optional

                    return false;
                }

                return true;
            }
        });

        try
        {
            T client;

            try
            {
                if (CanCreateWith(typeof(RestClient)))
                {
                    var credentials = await GetCredentialsAsync().ConfigureAwait(false);
                    var endpointUrl = AzureEnvironment.AzureGlobalCloud.GetEndpointUrl(azureEndpoint);

                    var restClient = RestClient.Configure()
                        .WithBaseUri(endpointUrl)
                        .WithCredentials(credentials)
                        .WithDelegatingHandler(httpClientFactory)
                        .Build();

                    client = (T)Activator.CreateInstance(typeof(T), new object[]
                    {
                            restClient
                    });
                }
                else if (CanCreateWith(typeof(ServiceClientCredentials), typeof(DelegatingHandler)))
                {
                    var credentials = await GetCredentialsAsync().ConfigureAwait(false);

                    client = (T)Activator.CreateInstance(typeof(T), new object[]
                    {
                            credentials,
                            httpClientFactory.CreateMessageHandler() as DelegatingHandler
                    });
                }
                else
                {
                    throw new NotSupportedException($"Clients of type '{typeof(T)}' are not supported");
                }
            }
            catch (TypeInitializationException)
            {
                throw;
            }
            catch (Exception exc)
            {
                throw new TypeInitializationException(typeof(RestClient).FullName, exc);
            }

            // if a subscription id was provided by the caller
            // set the corresponding property on the client instance

            if (subscriptionId.HasValue
                && typeof(T).TryGetProperty("SubscriptionId", out PropertyInfo subscriptionPropertyInfo)
                && subscriptionPropertyInfo.PropertyType == typeof(string))
                subscriptionPropertyInfo.SetValue(client, subscriptionId.Value.ToString());

            // check if the client instance has a tenant id property
            // which is not yet initialized - if so, use the tenant id
            // provided by the session options and initialize the client

            if (typeof(T).TryGetProperty("TenantID", out PropertyInfo tenantPropertyInfo)
                && tenantPropertyInfo.PropertyType == typeof(string)
                && string.IsNullOrEmpty(tenantPropertyInfo.GetValue(client) as string))
                tenantPropertyInfo.SetValue(client, azureSessionOptions.TenantId);

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
