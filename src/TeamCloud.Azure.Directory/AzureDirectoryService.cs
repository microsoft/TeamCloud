/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Rest.Azure.OData;
using TeamCloud.Http;

namespace TeamCloud.Azure.Directory
{
    public interface IAzureDirectoryService
    {
        Task<Guid?> GetUserIdAsync(string identifier);

        Task<Guid?> GetGroupIdAsync(string identifier);

        Task<AzureServicePrincipal> CreateServicePrincipalAsync(string name, string password = null);

        Task<AzureServicePrincipal> GetServicePrincipalAsync(string name);

        Task DeleteServicePrincipalAsync(string name);
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

            using var client = azureSessionService
                .CreateClient<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint);

            identifier = identifier
                .Replace("%3A", ":", StringComparison.OrdinalIgnoreCase)
                .Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);

            // assume user first
            var userInner = await GetUserInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(userInner?.ObjectId))
                return Guid.Parse(userInner.ObjectId);

            // otherwise try to find a service pricipal
            var principalInner = await GetServicePrincipalInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(principalInner?.ObjectId))
                return Guid.Parse(principalInner.ObjectId);

            // not a user name or objectId, and not a service pricipal name, appId, or objectId
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
                name = ServicePrincipalNamePrefix + name.Substring(ServicePrincipalNamePrefix.Length);
            else
                name = ServicePrincipalNamePrefix + name;

            return name.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        // public async Task<AzureServicePrincipal> CreateServicePrincipalAsync(string name, string password = null)
        // {
        //     if (string.IsNullOrWhiteSpace(name))
        //         throw new ArgumentException("Must not NULL or WHITESPACE", nameof(name));

        //     name = SanitizeServicePrincipalName(name);

        //     try
        //     {
        //         var servicePrincipal = await azureSessionService.CreateSession()
        //             .ServicePrincipals
        //             .Define(name)
        //                 .WithNewApplication($"http://{name}")
        //                 .CreateAsync()
        //                 .ConfigureAwait(false);

        //         return await ResetServicePrincipalAsync(name, password)
        //             .ConfigureAwait(false);
        //     }
        //     catch
        //     {
        //         // we created the service principal as part of this call
        //         // as the password set operation failed we will try
        //         // to clean up our mess

        //         await DeleteServicePrincipalAsync(name)
        //             .ConfigureAwait(false);

        //         throw;
        //     }
        // }

        // public async Task<AzureServicePrincipal> ResetServicePrincipalAsync(string name, string password = null)
        // {
        //     if (string.IsNullOrWhiteSpace(name))
        //         throw new ArgumentException("Must not NULL or WHITESPACE", nameof(name));

        //     var serviceApplication = await GetServiceApplicationInternalAsync(name)
        //         .ConfigureAwait(false);

        //     if (serviceApplication is null)
        //         throw new ArgumentOutOfRangeException(nameof(name));

        //     var servicePrincipal = await GetServicePrincipalAsync(name)
        //         .ConfigureAwait(false);

        //     var token = await azureSessionService
        //         .AcquireTokenAsync(AzureEndpoint.GraphEndpoint)
        //         .ConfigureAwait(false);

        //     if (string.IsNullOrEmpty(password))
        //         password = CreateServicePrincipalPassword();

        //     var startDate = DateTime.UtcNow;
        //     var endDate = startDate.AddYears(1);

        //     var patchPayloads = new List<object>()
        //     {
        //         new
        //         {
        //             passwordCredentials = new[]
        //             {
        //                 new
        //                 {
        //                     startDate,
        //                     endDate,
        //                     keyId = Guid.NewGuid(),
        //                     value = password,
        //                     customKeyIdentifier = Convert.ToBase64String(servicePrincipal.ObjectId.ToByteArray())
        //                 }
        //             }
        //         },
        //         new
        //         {
        //             requiredResourceAccess = new[]
        //             {
        //                 new
        //                 {
        //                     resourceAppId = "00000003-0000-0000-c000-000000000000",
        //                     resourceAccess = new[]
        //                     {
        //                         new
        //                         {
        //                             id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
        //                             type = "Scope"
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     };

        //     var patchTasks = patchPayloads
        //         .Select(payload => $"https://graph.windows.net/{azureSessionService.Options.TenantId}/applications/{serviceApplication.Inner.ObjectId}"
        //                             .SetQueryParam("api-version", "1.6")
        //                             .WithOAuthBearerToken(token)
        //                             .PatchJsonAsync(payload));

        //     await Task
        //         .WhenAll(patchTasks)
        //         .ConfigureAwait(false);

        //     servicePrincipal.Password = password;

        //     return servicePrincipal;
        // }

        // public async Task<AzureServicePrincipal> GetServicePrincipalAsync(string name)
        // {
        //     if (string.IsNullOrWhiteSpace(name))
        //         throw new ArgumentException("Must not NULL or WHITESPACE", nameof(name));

        //     var servicePrincipal = await GetServicePrincipalInternalAsync(name)
        //         .ConfigureAwait(false);

        //     if (servicePrincipal is null)
        //         return null;

        //     var serviceApplication = await GetServiceApplicationInternalAsync(name)
        //         .ConfigureAwait(false);

        //     if (serviceApplication is null)
        //         return null;

        //     var azureServicePrincipal = new AzureServicePrincipal()
        //     {
        //         ObjectId = Guid.Parse(servicePrincipal.Id),
        //         ApplicationId = Guid.Parse(servicePrincipal.ApplicationId),
        //         Name = servicePrincipal.Name
        //     };

        //     var token = await azureSessionService
        //         .AcquireTokenAsync(AzureEndpoint.GraphEndpoint)
        //         .ConfigureAwait(false);

        //     var json = await $"https://graph.windows.net/{azureSessionService.Options.TenantId}/applications/{serviceApplication.Inner.ObjectId}"
        //         .SetQueryParam("api-version", "1.6")
        //         .WithOAuthBearerToken(token)
        //         .GetJObjectAsync()
        //         .ConfigureAwait(false);

        //     var identifier = Convert.ToBase64String(azureServicePrincipal.ObjectId.ToByteArray());
        //     var expiresOn = json.SelectToken($"$.value[?(@.customKeyIdentifier == '{identifier}')].endDate")?.ToString();

        //     if (!string.IsNullOrEmpty(expiresOn) && DateTime.TryParse(expiresOn, out var expiresOnDateTime))
        //         azureServicePrincipal.ExpiresOn = expiresOnDateTime;

        //     return azureServicePrincipal;
        // }

        // private async Task<IServicePrincipal> GetServicePrincipalInternalAsync(string name)
        // {
        //     if (string.IsNullOrWhiteSpace(name))
        //         throw new ArgumentException("Must not NULL or WHITESPACE", nameof(name));

        //     name = SanitizeServicePrincipalName(name);

        //     try
        //     {
        //         return await azureSessionService.CreateSession()
        //             .ServicePrincipals
        //             .GetByNameAsync(name)
        //             .ConfigureAwait(false);
        //     }
        //     catch
        //     {
        //         return null;
        //     }
        // }

        // private async Task<IActiveDirectoryApplication> GetServiceApplicationInternalAsync(string name)
        // {
        //     if (string.IsNullOrWhiteSpace(name))
        //         throw new ArgumentException("Must not NULL or WHITESPACE", nameof(name));

        //     name = SanitizeServicePrincipalName(name);

        //     try
        //     {
        //         return await azureSessionService.CreateSession()
        //             .ActiveDirectoryApplications
        //             .GetByNameAsync(name)
        //             .ConfigureAwait(false);
        //     }
        //     catch
        //     {
        //         return null;
        //     }
        // }

        // public async Task DeleteServicePrincipalAsync(string name)
        // {
        //     if (string.IsNullOrWhiteSpace(name))
        //         throw new ArgumentException("Must not NULL or WHITESPACE", nameof(name));


        //     var session = azureSessionService.CreateSession();

        //     var serviceApplication = await GetServiceApplicationInternalAsync(name)
        //         .ConfigureAwait(false);

        //     await session.ActiveDirectoryApplications.DeleteByIdAsync(serviceApplication.Id)
        //         .ConfigureAwait(false);

        //     var servicePrincipal = await GetServicePrincipalInternalAsync(name)
        //         .ConfigureAwait(false);

        //     await session.ServicePrincipals.DeleteByIdAsync(servicePrincipal.Id)
        //         .ConfigureAwait(false);
        // }

        public async Task<AzureServicePrincipal> CreateServicePrincipalAsync(string name, string password = null)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            name = SanitizeServicePrincipalName(name);

            using var client = azureSessionService
                .CreateClient<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint);

            password ??= CreateServicePrincipalPassword();

            var expiresOn = DateTime.UtcNow.AddYears(1);

            var parameters = new ApplicationCreateParameters()
            {
                DisplayName = name,
                // IdentifierUris = new List<string> { $"http://{name}" },
                Homepage = $"http://{name}",
                PasswordCredentials = new List<PasswordCredential> {
                    new PasswordCredential {
                        StartDate = DateTime.UtcNow,
                        EndDate = expiresOn,
                        KeyId = Guid.NewGuid().ToString(),
                        Value = password,
                        CustomKeyIdentifier = Encoding.ASCII.GetBytes(name)
                    }
                },
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

            try
            {
                var application = await client.Applications
                    .CreateAsync(parameters)
                    .ConfigureAwait(false);

                var principalId = await client.Applications
                    .GetServicePrincipalsIdByAppIdAsync(application.AppId)
                    .ConfigureAwait(false);

                var principal = await client.ServicePrincipals
                    .GetAsync(principalId.Value)
                    .ConfigureAwait(false);

                var azureServicePrincipal = new AzureServicePrincipal()
                {
                    ObjectId = Guid.Parse(principal.ObjectId),
                    ApplicationId = Guid.Parse(principal.AppId),
                    Name = principal.ServicePrincipalNames.FirstOrDefault(),
                    Password = password,
                    ExpiresOn = expiresOn
                };

                return azureServicePrincipal;
            }
            catch (GraphErrorException ex)
            {
                throw new Exception(ex.Body.Message);
                // throw;
            }
        }


        public async Task<AzureServicePrincipal> GetServicePrincipalAsync(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            name = SanitizeServicePrincipalName(name);

            using var client = azureSessionService
                .CreateClient<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint);

            var principal = await GetServicePrincipalInnerAsync(client, name)
                .ConfigureAwait(false);

            if (principal is null)
                return null;

            var application = await GetServiceApplicationInnerAsync(client, principal.AppId)
                .ConfigureAwait(false);

            if (application is null)
                return null;

            return new AzureServicePrincipal
            {
                ObjectId = Guid.Parse(principal.ObjectId),
                ApplicationId = Guid.Parse(principal.AppId),
                Name = principal.ServicePrincipalNames.FirstOrDefault(),
                ExpiresOn = application.PasswordCredentials.FirstOrDefault(c => c.CustomKeyIdentifier == Encoding.ASCII.GetBytes(name))?.EndDate
            };
        }


        public async Task DeleteServicePrincipalAsync(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            name = SanitizeServicePrincipalName(name);

            using var client = azureSessionService
                .CreateClient<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint);

            var principal = await GetServicePrincipalInnerAsync(client, name)
                .ConfigureAwait(false);

            if (!(principal is null))
            {
                var application = await GetServiceApplicationInnerAsync(client, principal.AppId)
                    .ConfigureAwait(false);

                if (!(application is null))
                    await client.Applications
                        .DeleteAsync(application.ObjectId)
                        .ConfigureAwait(false);

                await client.ServicePrincipals
                    .DeleteAsync(principal.ObjectId)
                    .ConfigureAwait(false);
            }
        }
    }
}
