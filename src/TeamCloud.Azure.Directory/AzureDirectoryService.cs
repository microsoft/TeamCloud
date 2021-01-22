/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Rest.Azure.OData;

namespace TeamCloud.Azure.Directory
{
    public interface IAzureDirectoryService
    {
        Task<Guid?> GetUserIdAsync(string identifier);

        Task<Guid?> GetGroupIdAsync(string identifier);

        Task<AzureServicePrincipal> CreateServicePrincipalAsync(string name, string password = null);

        Task<AzureServicePrincipal> GetServicePrincipalAsync(string identifier);

        Task DeleteServicePrincipalAsync(string name);

        Task<IEnumerable<string>> GetServicePrincipalRedirectUrlsAsync(string servicePrincipalIdentifier);

        Task<IEnumerable<string>> SetServicePrincipalRedirectUrlsAsync(string servicePrincipalIdentifier, IEnumerable<string> redirectUrls);
    }

    public class AzureDirectoryService : IAzureDirectoryService
    {
        private readonly IAzureSessionService azureSessionService;

        public AzureDirectoryService(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        public async Task<Guid?> GetUserIdAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            identifier = identifier
                .Replace("%3A", ":", StringComparison.OrdinalIgnoreCase)
                .Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);

            // assume user first
            var userInner = await GetUserInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(userInner?.ObjectId))
                return Guid.Parse(userInner.ObjectId);

            // otherwise try to find a service principal
            var principalInner = await GetServicePrincipalInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(principalInner?.ObjectId))
                return Guid.Parse(principalInner.ObjectId);

            // not a user name or objectId, and not a service principal name, appId, or objectId
            return null;
        }

        public Task<Guid?> GetGroupIdAsync(string identifier)
        {
            throw new NotImplementedException();
        }

        private static async Task<UserInner> GetUserInnerAsync(GraphRbacManagementClient client, string identifier)
        {
            if (identifier.StartsWithHttp())
                return null;

            if (!(identifier.IsGuid() || identifier.IsEMail()))
                return null;

            if (identifier.IsEMail())
            {
                var domains = await client.Domains
                    .ListAsync()
                    .ConfigureAwait(false);

                var hasVerifiedDomain = domains
                    .Where(d => d.IsVerified.HasValue && d.IsVerified.Value)
                    .Any(d => identifier.EndsWith($"@{d.Name}", StringComparison.OrdinalIgnoreCase));

                if (!hasVerifiedDomain)
                {
                    var defaultDomain = domains
                        .First(d => d.IsDefault.HasValue && d.IsDefault.Value);

                    identifier = $"{identifier.Replace("@", "_", StringComparison.OrdinalIgnoreCase)}#EXT#@{defaultDomain.Name}";
                }
            }

            try
            {
                return await client.Users
                    .GetAsync(identifier)
                    .ConfigureAwait(false);
            }
            catch (GraphErrorException)
            {
                return null;
            }
        }

        private static async Task<ServicePrincipalInner> GetServicePrincipalInnerAsync(GraphRbacManagementClient client, string identifier)
        {
            var query = new ODataQuery<ServicePrincipalInner>();

            var httpIdentifier = $"http://{identifier}";
            var httpsIdentifier = $"https://{identifier}";

            if (identifier.IsGuid())
                query.SetFilter(sp => sp.ObjectId == identifier || sp.AppId == identifier || sp.ServicePrincipalNames.Contains(identifier));
            else if (!identifier.StartsWithHttp())
                query.SetFilter(sp => sp.ServicePrincipalNames.Contains(identifier) || sp.ServicePrincipalNames.Contains(httpIdentifier) || sp.ServicePrincipalNames.Contains(httpsIdentifier));
            else
                query.SetFilter(sp => sp.ServicePrincipalNames.Contains(identifier));

            try
            {
                var page = await client.ServicePrincipals
                    .ListAsync(query)
                    .ConfigureAwait(false);

                var principal = page.FirstOrDefault();

                while (principal is null && !string.IsNullOrEmpty(page?.NextPageLink))
                {
                    page = await client.ServicePrincipals
                        .ListNextAsync(page.NextPageLink)
                        .ConfigureAwait(false);

                    principal = page.FirstOrDefault();
                }

                return principal;

            }
            catch (GraphErrorException)
            {
                return null;
            }
        }

        private static async Task<ApplicationInner> GetServiceApplicationInnerAsync(GraphRbacManagementClient client, string identifier)
        {
            var query = new ODataQuery<ApplicationInner>();

            var httpIdentifier = $"http://{identifier}";
            var httpsIdentifier = $"https://{identifier}";

            if (identifier.IsGuid())
                query.SetFilter(a => a.ObjectId == identifier || a.AppId == identifier);
            else if (!identifier.StartsWithHttp())
                query.SetFilter(a => a.IdentifierUris.Contains(httpIdentifier) || a.IdentifierUris.Contains(httpsIdentifier));
            else
                query.SetFilter(a => a.IdentifierUris.Contains(identifier));

            try
            {
                var page = await client.Applications
                    .ListAsync(query)
                    .ConfigureAwait(false);

                var application = page.FirstOrDefault();

                while (application is null && !string.IsNullOrEmpty(page?.NextPageLink))
                {
                    page = await client.Applications
                        .ListNextAsync(page.NextPageLink)
                        .ConfigureAwait(false);

                    application = page.FirstOrDefault();
                }

                return application;

            }
            catch (GraphErrorException)
            {
                return null;
            }
        }

        private static string CreateServicePrincipalPassword()
        {
            using var cryptRNG = new RNGCryptoServiceProvider();

            byte[] tokenBuffer = new byte[20];
            cryptRNG.GetBytes(tokenBuffer);

            return Convert.ToBase64String(tokenBuffer);
        }

        private static string SanitizeServicePrincipalName(string name)
        {
            const string ServicePrincipalNamePrefix = "TeamCloud/";

            if (name.StartsWith(ServicePrincipalNamePrefix, StringComparison.OrdinalIgnoreCase))
                name = ServicePrincipalNamePrefix + name[ServicePrincipalNamePrefix.Length..];
            else
                name = ServicePrincipalNamePrefix + name;

            return name.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<AzureServicePrincipal> CreateServicePrincipalAsync(string name, string password = null)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            name = SanitizeServicePrincipalName(name);

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            password ??= CreateServicePrincipalPassword();

            var expiresOn = DateTime.UtcNow.AddYears(1);

            var parameters = new ApplicationCreateParameters()
            {
                DisplayName = name,
                AvailableToOtherTenants = false,
                IdentifierUris = new List<string> { $"http://{name}" },
                RequiredResourceAccess = new List<RequiredResourceAccess> {
                    new RequiredResourceAccess {
                        ResourceAppId = "00000003-0000-0000-c000-000000000000",
                        ResourceAccess = new List<ResourceAccess> {
                            new ResourceAccess {
                                Id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
                                Type = "Scope"
                            }
                        }
                    }
                }
            };

            var application = await client.Applications
                .CreateAsync(parameters)
                .ConfigureAwait(false);

            var principal = await client.ServicePrincipals
                .CreateAsync(new ServicePrincipalCreateParameters { AppId = application.AppId })
                .ConfigureAwait(false);

            await client.Applications
                .UpdatePasswordCredentialsAsync(application.ObjectId, new List<PasswordCredential> {
                    new PasswordCredential {
                        StartDate = DateTime.UtcNow,
                        EndDate = expiresOn,
                        KeyId = Guid.NewGuid().ToString(),
                        Value = password,
                        CustomKeyIdentifier = Guid.Parse(principal.ObjectId).ToByteArray()
                    }
                }).ConfigureAwait(false);

            var azureServicePrincipal = new AzureServicePrincipal()
            {
                ObjectId = Guid.Parse(principal.ObjectId),
                ApplicationId = Guid.Parse(principal.AppId),
                TenantId = Guid.Parse(principal.AppOwnerTenantId),
                Name = principal.ServicePrincipalNames.FirstOrDefault(),
                Password = password,
                ExpiresOn = expiresOn
            };

            return azureServicePrincipal;
        }


        public async Task<AzureServicePrincipal> GetServicePrincipalAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            identifier = SanitizeServicePrincipalName(identifier);

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            var principal = await GetServicePrincipalInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (principal is null)
                return null;

            var application = await GetServiceApplicationInnerAsync(client, principal.AppId)
                .ConfigureAwait(false);

            if (application is null)
                return null;

            var expiresOn = application.PasswordCredentials
                .Where(c => c.CustomKeyIdentifier == Guid.Parse(principal.ObjectId).ToByteArray())
                .OrderBy(c => c.EndDate)
                .FirstOrDefault()?.EndDate;

            return new AzureServicePrincipal
            {
                ObjectId = Guid.Parse(principal.ObjectId),
                ApplicationId = Guid.Parse(principal.AppId),
                TenantId = Guid.Parse(principal.AppOwnerTenantId),
                Name = principal.ServicePrincipalNames.FirstOrDefault(),
                ExpiresOn = expiresOn
            };
        }


        public async Task DeleteServicePrincipalAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            identifier = SanitizeServicePrincipalName(identifier);

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            var principal = await GetServicePrincipalInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (principal != null)
            {
                var application = await GetServiceApplicationInnerAsync(client, principal.AppId)
                    .ConfigureAwait(false);

                if (application != null)
                {
                    await client.Applications
                        .DeleteAsync(application.ObjectId)
                        .ConfigureAwait(false);
                }
            }
        }

        public async Task<IEnumerable<string>> GetServicePrincipalRedirectUrlsAsync(string servicePrincipalIdentifier)
        {
            if (servicePrincipalIdentifier is null)
                throw new ArgumentNullException(nameof(servicePrincipalIdentifier));

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            var principal = await GetServicePrincipalInnerAsync(client, servicePrincipalIdentifier)
                .ConfigureAwait(false);

            if (principal != null)
            {
                var application = await GetServiceApplicationInnerAsync(client, principal.AppId)
                    .ConfigureAwait(false);

                return application?.ReplyUrls;
            }

            return null;
        }

        public async Task<IEnumerable<string>> SetServicePrincipalRedirectUrlsAsync(string servicePrincipalIdentifier, IEnumerable<string> redirectUrls)
        {
            if (servicePrincipalIdentifier is null)
                throw new ArgumentNullException(nameof(servicePrincipalIdentifier));

            if (redirectUrls is null)
                throw new ArgumentNullException(nameof(redirectUrls));

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            var principal = await GetServicePrincipalInnerAsync(client, servicePrincipalIdentifier)
                .ConfigureAwait(false);

            if (principal != null)
            {
                var application = await GetServiceApplicationInnerAsync(client, principal.AppId)
                    .ConfigureAwait(false);

                if (application != null)
                {
                    var parameters = new ApplicationUpdateParameters()
                    {
                        ReplyUrls = redirectUrls.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                    };

                    await client.Applications
                        .PatchAsync(application.ObjectId, parameters)
                        .ConfigureAwait(false);
                }
            }

            return await GetServicePrincipalRedirectUrlsAsync(servicePrincipalIdentifier)
                .ConfigureAwait(false);
        }
    }
}
