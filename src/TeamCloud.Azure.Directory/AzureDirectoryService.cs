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
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.OData;

namespace TeamCloud.Azure.Directory
{
    public interface IAzureDirectoryService
    {
        Task<bool> IsUserAsync(string identifier);

        Task<Guid?> GetUserIdAsync(string identifier);

        Task<bool> IsGroupAsync(string identifier);

        Task<Guid?> GetGroupIdAsync(string identifier);

        IAsyncEnumerable<Guid> GetGroupMembersAsync(string identifier, bool resolveAllGroups = false);

        Task<string> GetDisplayNameAsync(string identifier);

        Task<string> GetLoginNameAsync(string identifier);

        Task<string> GetMailAddressAsync(string identifier);

        Task<AzureServicePrincipal> CreateServicePrincipalAsync(string name, string password = null);

        Task<AzureServicePrincipal> RefreshServicePrincipalAsync(string identifier, string password = null);

        Task<AzureServicePrincipal> GetServicePrincipalAsync(string identifier);

        Task DeleteServicePrincipalAsync(string name);

        Task<IEnumerable<string>> GetServicePrincipalRedirectUrlsAsync(string servicePrincipalIdentifier);

        Task<IEnumerable<string>> SetServicePrincipalRedirectUrlsAsync(string servicePrincipalIdentifier, IEnumerable<string> redirectUrls);
    }

    public class AzureDirectoryService : IAzureDirectoryService
    {
        private const string SECRET_DESCRIPTION = "Managed by TeamCloud";

        private readonly IAzureSessionService azureSessionService;

        public AzureDirectoryService(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        private static string SanitizeIdentifier(string identifier) => identifier?
            .Replace("%3A", ":", StringComparison.OrdinalIgnoreCase)?
            .Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);

        public Task<bool> IsUserAsync(string identifier)
            => GetUserIdAsync(identifier).ContinueWith(task => task.Result.HasValue, TaskContinuationOptions.OnlyOnRanToCompletion);

        public async Task<Guid?> GetUserIdAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            identifier = SanitizeIdentifier(identifier);

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

        public Task<bool> IsGroupAsync(string identifier)
            => GetGroupIdAsync(identifier).ContinueWith(task => task.Result.HasValue, TaskContinuationOptions.OnlyOnRanToCompletion);

        public async Task<Guid?> GetGroupIdAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            var groupInner = await GetGroupInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(groupInner?.ObjectId))
                return Guid.Parse(groupInner?.ObjectId);

            return null;
        }

        public async IAsyncEnumerable<Guid> GetGroupMembersAsync(string identifier, bool resolveAllGroups = false)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            var groupId = await GetGroupIdAsync(identifier)
                .ConfigureAwait(false);

            if (groupId != null)
            {
                var uniqueMembers = new HashSet<Guid>();

                var memberIds = FetchMemberIds(groupId.Value)
                    .ConfigureAwait(false);

                await foreach (var memberId in memberIds)
                {
                    if (resolveAllGroups)
                    {
                        var subGroupId = await GetGroupIdAsync(memberId.ToString())
                            .ConfigureAwait(false);

                        if (subGroupId.HasValue)
                        {
                            var subMemberIds = GetGroupMembersAsync(subGroupId.ToString(), resolveAllGroups)
                                .ConfigureAwait(false);

                            await foreach (var subMemberId in subMemberIds)
                                if (uniqueMembers.Add(subMemberId))
                                    yield return subMemberId;
                        }
                    }

                    if (uniqueMembers.Add(memberId))
                        yield return memberId;
                }
            }

            async IAsyncEnumerable<Guid> FetchMemberIds(Guid groupId)
            {
                using var client = await azureSessionService
                    .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                    .ConfigureAwait(false);

                IPage<DirectoryObject> page = null;

                while (true)
                {
                    if (page is null)
                    {
                        page = await client.Groups
                            .GetGroupMembersAsync(groupId.ToString())
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        page = await client.Groups
                            .GetGroupMembersNextAsync(page.NextPageLink)
                            .ConfigureAwait(false);
                    }

                    foreach (var memberId in page.Where(m => !m.DeletionTimestamp.HasValue).Select(m => Guid.Parse(m.ObjectId)))
                        yield return memberId;

                    if (string.IsNullOrEmpty(page.NextPageLink))
                        break;
                }
            }
        }

        public async Task<string> GetDisplayNameAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            identifier = SanitizeIdentifier(identifier);

            // assume user first
            var userInner = await GetUserInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(userInner?.DisplayName))
                return userInner.DisplayName;

            // otherwise try to find a service principal
            var principalInner = await GetServicePrincipalInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(principalInner?.DisplayName))
                return principalInner.DisplayName;

            return null;
        }

        public async Task<string> GetLoginNameAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            identifier = SanitizeIdentifier(identifier);

            var userInner = await GetUserInnerAsync(client, identifier)
                .ConfigureAwait(false);

            return userInner?.SignInNames?.FirstOrDefault()?.Value
                ?? userInner.Mail;
        }

        public async Task<string> GetMailAddressAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            using var client = await azureSessionService
                .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            identifier = SanitizeIdentifier(identifier);

            // assume user first
            var userInner = await GetUserInnerAsync(client, identifier)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(userInner?.Mail))
                return userInner.Mail;

            return null;
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

        private static async Task<ADGroupInner> GetGroupInnerAsync(GraphRbacManagementClient client, string identifier)
        {
            if (!identifier.IsGuid())
                return null;

            try
            {
                return await client.Groups
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
            var fqIdentifier = $"api://{identifier}";

            if (identifier.IsGuid())
                query.SetFilter(sp => sp.ObjectId == identifier || sp.AppId == identifier || sp.ServicePrincipalNames.Contains(identifier));
            else if (!identifier.StartsWith("api://"))
                query.SetFilter(sp => sp.ServicePrincipalNames.Contains(identifier) || sp.ServicePrincipalNames.Contains(fqIdentifier));
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

            try
            {
                name = SanitizeServicePrincipalName(name);

                using var client = await azureSessionService
                    .CreateClientAsync<GraphRbacManagementClient>(AzureEndpoint.GraphEndpoint)
                    .ConfigureAwait(false);

                var parameters = new ApplicationCreateParameters()
                {
                    DisplayName = name,
                    AvailableToOtherTenants = false,
                    IdentifierUris = new List<string> { $"api://{name}" },
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

                var expiresOn = DateTime.UtcNow.AddYears(1);

                password ??= CreateServicePrincipalPassword();

                await client.Applications
                    .UpdatePasswordCredentialsAsync(application.ObjectId, new List<PasswordCredential> {
                    new PasswordCredential {
                        StartDate = DateTime.UtcNow,
                        EndDate = expiresOn,
                        KeyId = Guid.NewGuid().ToString(),
                        Value = password,
                        CustomKeyIdentifier = Encoding.UTF8.GetBytes(SECRET_DESCRIPTION)
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
            catch
            {
                throw;
            }
        }

        public async Task<AzureServicePrincipal> RefreshServicePrincipalAsync(string identifier, string password = null)
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

            var expiresOn = DateTime.UtcNow.AddYears(1);

            password ??= CreateServicePrincipalPassword();

            application.PasswordCredentials = application.PasswordCredentials
                .Where(cred => !Encoding.UTF8.GetBytes(SECRET_DESCRIPTION).SequenceEqual(cred.CustomKeyIdentifier ?? Enumerable.Empty<byte>()))
                .Append(new PasswordCredential
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = expiresOn,
                    KeyId = Guid.NewGuid().ToString(),
                    Value = password,
                    CustomKeyIdentifier = Encoding.UTF8.GetBytes(SECRET_DESCRIPTION)
                }).ToList();

            await client.Applications
                .UpdatePasswordCredentialsAsync(application.ObjectId, application.PasswordCredentials)
                .ConfigureAwait(false);

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

            var customKeyIdentifier = Guid.Parse(principal.ObjectId).ToByteArray();

            var expiresOn = application.PasswordCredentials
                .SingleOrDefault(c => c.KeyId.Equals(principal.ObjectId, StringComparison.Ordinal))?.EndDate;

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
