/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using TeamCloud.Azure.Identity;

namespace TeamCloud.Microsoft.Graph;

public class GraphService : IGraphService
{

    private const string userSelect = "id,userPrincipalName,displayName,givenName,surname,mail,otherMails,identities,deletedDateTime,companyName,jobTitle,preferredLanguage,userType,department";
    private const string groupSelect = "id,displayName,mail,deletedDateTime";
    private const string memberSelect = "id,displayName,mail,deletedDateTime";
    private const string servicePrincipalSelect = "id,displayName,appId,appDisplayName,alternativeNames,deletedDateTime,servicePrincipalNames,servicePrincipalType,replyUrls";
    private const string applicationSelect = "id,displayName,appId,deletedDateTime,identifierUris,uniqueName,passwordCredentials";

    private const string SECRET_DESCRIPTION = "Managed by TeamCloud";
    private readonly GraphServiceClient graphClient;

    public GraphService(ITeamCloudCredentialOptions teamCloudCredentialOptions = null)
    {
        graphClient = new GraphServiceClient(new TeamCloudCredential(teamCloudCredentialOptions));
    }

    private static string SanitizeIdentifier(string identifier) => identifier?
        .Replace("%3A", ":", StringComparison.OrdinalIgnoreCase)?
        .Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);

    public async Task<string> GetDisplayNameAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeIdentifier(identifier);

        // assume user first
        var user = await GetUserInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(user?.DisplayName))
            return user.DisplayName;

        // otherwise try to find a service principal
        var principal = await GetServicePrincipalInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(principal?.DisplayName))
            return principal.DisplayName;

        return null;
    }

    public async Task<string> GetGroupIdAsync(string identifier)
    {
        if (!identifier.IsGuid())
            return null;

        try
        {
            var group = await graphClient
                .Groups[identifier]
                .Request()
                .GetAsync()
                .ConfigureAwait(false);

            return group?.Id;
        }
        catch (ServiceException)
        {
            return null;
        }
    }

    public async IAsyncEnumerable<string> GetGroupMembersAsync(string identifier, bool resolveAllGroups = false)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        var groupId = await GetGroupIdAsync(identifier)
            .ConfigureAwait(false);

        if (groupId is not null)
        {
            if (resolveAllGroups)
            {
                IGroupTransitiveMembersCollectionWithReferencesPage page = null;

                while (true)
                {
                    if (page is null)
                    {
                        page = await graphClient
                            .Groups[groupId]
                            .TransitiveMembers
                            .Request()
                            .Header("ConsistencyLevel", "eventual")
                            .Select(memberSelect)
                            .GetAsync()
                            .ConfigureAwait(false);
                    }
                    else if (page.NextPageRequest is not null)
                    {
                        page = await page
                            .NextPageRequest
                            .GetAsync()
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        break;
                    }

                    foreach (var memberId in page.CurrentPage.Where(m => !m.DeletedDateTime.HasValue).Select(m => m.Id))
                        yield return memberId;
                }
            }
            else
            {
                IGroupMembersCollectionWithReferencesPage page = null;

                while (true)
                {
                    if (page is null)
                    {
                        page = await graphClient
                            .Groups[groupId]
                            .Members
                            .Request()
                            .Header("ConsistencyLevel", "eventual")
                            .Select(memberSelect)
                            .GetAsync()
                            .ConfigureAwait(false);
                    }
                    else if (page.NextPageRequest is not null)
                    {
                        page = await page
                            .NextPageRequest
                            .GetAsync()
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        break;
                    }

                    foreach (var memberId in page.CurrentPage.Where(m => !m.DeletedDateTime.HasValue).Select(m => m.Id))
                        yield return memberId;
                }
            }
        }
    }

    public async Task<string> GetLoginNameAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeIdentifier(identifier);

        var user = await GetUserInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        return user?.Mail;
    }

    public async Task<string> GetMailAddressAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeIdentifier(identifier);

        var user = await GetUserInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        return user?.Mail;
    }

    public async Task<string> GetUserIdAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeIdentifier(identifier);

        // assume user first
        var user = await GetUserInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(user?.Id))
            return user.Id;

        // otherwise try to find a service principal
        var principal = await GetServicePrincipalInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(principal?.Id))
            return principal.Id;

        // not a user name or idd, and not a service principal name, appId, or id
        return null;
    }

    public Task<bool> IsGroupAsync(string identifier)
        => GetGroupIdAsync(identifier).ContinueWith(t => t.Result is not null, TaskContinuationOptions.OnlyOnRanToCompletion);

    public Task<bool> IsUserAsync(string identifier)
        => GetUserIdAsync(identifier).ContinueWith(t => t.Result is not null, TaskContinuationOptions.OnlyOnRanToCompletion);

    public async Task<AzureServicePrincipal> CreateServicePrincipalAsync(string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        try
        {
            name = SanitizeServicePrincipalName(name);

            var application = new Application
            {
                DisplayName = name,
                SignInAudience = "AzureADMyOrg",
                RequiredResourceAccess = new List<RequiredResourceAccess> {
                    new RequiredResourceAccess {
                        ResourceAppId = "00000003-0000-0000-c000-000000000000",
                        ResourceAccess = new List<ResourceAccess> {
                            new ResourceAccess {
                                Id = Guid.Parse("e1fe6dd8-ba31-4d61-89e7-88639da4683d"), // User.Read
                                Type = "Scope"
                            }
                        }
                    }
                },
                IdentifierUris = new List<string>
                {
                    $"api://{name}"
                }
            };

            application = await graphClient.Applications
                .Request()
                .AddAsync(application)
                .ConfigureAwait(false);

            var servicePrincipal = new ServicePrincipal
            {
                AppId = application.AppId
            };

            servicePrincipal = await graphClient.ServicePrincipals
                .Request()
                .AddAsync(servicePrincipal)
                .ConfigureAwait(false);
            
            var expiresOn = DateTimeOffset.UtcNow.AddYears(1);

            var passwordCredential = new PasswordCredential
            {
                StartDateTime = DateTimeOffset.UtcNow,
                EndDateTime = expiresOn,
                KeyId = Guid.NewGuid(),
                CustomKeyIdentifier = Encoding.UTF8.GetBytes(SECRET_DESCRIPTION)
            };

            passwordCredential = await graphClient
                .Applications[application.Id]
                .AddPassword(passwordCredential)
                .Request()
                .PostAsync()
                .ConfigureAwait(false);

            var azureServicePrincipal = new AzureServicePrincipal
            {
                Id = Guid.Parse(servicePrincipal.Id),
                AppId = Guid.Parse(servicePrincipal.AppId),
                TenantId = servicePrincipal.AppOwnerOrganizationId.GetValueOrDefault(),
                Name = servicePrincipal.ServicePrincipalNames.FirstOrDefault(),
                Password = passwordCredential.SecretText,
                ExpiresOn = expiresOn
            };

            return azureServicePrincipal;
        }
        catch
        {
            throw;
        }
    }

    public async Task DeleteServicePrincipalAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeServicePrincipalName(identifier);

        var servicePrincipal = await GetServicePrincipalInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (servicePrincipal is not null)
        {
            var application = await GetApplicationInternalAsync(graphClient, servicePrincipal.AppId)
                .ConfigureAwait(false);

            if (application is not null)
            {
                await graphClient
                    .Applications[application.Id]
                    .Request()
                    .DeleteAsync()
                    .ConfigureAwait(false);
            }
        }
    }

    public async Task<AzureServicePrincipal> GetServicePrincipalAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeServicePrincipalName(identifier);

        var servicePrincipal = await GetServicePrincipalInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (servicePrincipal is null)
            return null;

        var application = await GetApplicationInternalAsync(graphClient, servicePrincipal.AppId)
            .ConfigureAwait(false);

        if (application is null)
            return null;

        var customKeyIdentifier = Guid.Parse(servicePrincipal.Id).ToByteArray();

        var expiresOn = application.PasswordCredentials
            .SingleOrDefault(c => c.KeyId.GetValueOrDefault().ToString().Equals(servicePrincipal.Id, StringComparison.Ordinal))?.EndDateTime;

        var azureServicePrincipal = new AzureServicePrincipal
        {
            Id = Guid.Parse(servicePrincipal.Id),
            AppId = Guid.Parse(servicePrincipal.AppId),
            TenantId = servicePrincipal.AppOwnerOrganizationId.GetValueOrDefault(),
            Name = servicePrincipal.ServicePrincipalNames.FirstOrDefault(),
            ExpiresOn = expiresOn
        };

        return azureServicePrincipal;
    }

    public async Task<Dictionary<string, Dictionary<Guid, AccessType>>> GetResourcePermissionsAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeServicePrincipalName(identifier);

        var application = await GetApplicationInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (application is null)
            throw new ArgumentException($"Could not find application by identifier '{identifier}'", nameof(identifier));

        if (application.RequiredResourceAccess is null)
            return new Dictionary<string, Dictionary<Guid, AccessType>>();

        return application.RequiredResourceAccess
            .Where(rra => rra.ResourceAccess?.Any() ?? false)
            .GroupBy(rra => rra.ResourceAppId)
            .ToDictionary(grp => grp.Key, grp => grp.SelectMany(item => item.ResourceAccess).Where(ra => ra.Id.HasValue).ToDictionary(ra => ra.Id.Value, ra => Enum.Parse<AccessType>(ra.Type)));
    }

    public async Task<Dictionary<string, Dictionary<Guid, AccessType>>> SetResourcePermissionsAsync(string identifier, Dictionary<string, Dictionary<Guid, AccessType>> resourcePermissions)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeServicePrincipalName(identifier);

        var application = await GetApplicationInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (application is null)
            throw new ArgumentException($"Could not find application by identifier '{identifier}'", nameof(identifier));

        var requiredResourceAccess = Enumerable.Empty<RequiredResourceAccess>();

        if (resourcePermissions?.Any() ?? false)
        {
            requiredResourceAccess = resourcePermissions.Select(rp => new RequiredResourceAccess()
            {
                ResourceAppId = rp.Key,
                ResourceAccess = (rp.Value?.Any() ?? false)
                    ? rp.Value.Select(ra => new ResourceAccess() { Id = ra.Key, Type = ra.Value.ToString() })
                    : Enumerable.Empty<ResourceAccess>()
            });
        }

        var applicationPatch = new Application()
        {
            RequiredResourceAccess = requiredResourceAccess
        };

        await graphClient.Applications[application.Id]
            .Request()
            .UpdateAsync(applicationPatch)
            .ConfigureAwait(false);

        return await GetResourcePermissionsAsync(identifier)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetServicePrincipalRedirectUrlsAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeServicePrincipalName(identifier);

        var servicePrincipal = await GetServicePrincipalInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        return servicePrincipal?.ReplyUrls;
    }

    public async Task<AzureServicePrincipal> RefreshServicePrincipalAsync(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeServicePrincipalName(identifier);

        var servicePrincipal = await GetServicePrincipalInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (servicePrincipal is null)
            return null;

        var application = await GetApplicationInternalAsync(graphClient, servicePrincipal.AppId)
            .ConfigureAwait(false);

        if (application is null)
            return null;

        var expiresOn = DateTimeOffset.UtcNow.AddYears(1);

        var passwordCredential = application
            .PasswordCredentials
            .FirstOrDefault(cred => Encoding.UTF8.GetBytes(SECRET_DESCRIPTION).SequenceEqual(cred.CustomKeyIdentifier ?? Enumerable.Empty<byte>()));

        if (passwordCredential is not null && passwordCredential.KeyId.HasValue)
        {
            await graphClient
                .Applications[application.Id]
                .RemovePassword(passwordCredential.KeyId.Value)
                .Request()
                .PostAsync()
                .ConfigureAwait(false);
        }

        passwordCredential = new PasswordCredential
        {
            StartDateTime = DateTimeOffset.UtcNow,
            EndDateTime = expiresOn,
            KeyId = Guid.NewGuid(),
            CustomKeyIdentifier = Encoding.UTF8.GetBytes(SECRET_DESCRIPTION)
        };

        passwordCredential = await graphClient
            .Applications[application.Id]
            .AddPassword(passwordCredential)
            .Request()
            .PostAsync()
            .ConfigureAwait(false);

        var azureServicePrincipal = new AzureServicePrincipal
        {
            Id = Guid.Parse(servicePrincipal.Id), // TODO: change to Id
            AppId = Guid.Parse(servicePrincipal.AppId),
            TenantId = servicePrincipal.AppOwnerOrganizationId.GetValueOrDefault(),
            Name = servicePrincipal.ServicePrincipalNames.FirstOrDefault(),
            Password = passwordCredential.SecretText,
            ExpiresOn = expiresOn
        };

        return azureServicePrincipal;
    }

    public async Task<IEnumerable<string>> SetServicePrincipalRedirectUrlsAsync(string identifier, params string[] redirectUrls)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        identifier = SanitizeServicePrincipalName(identifier);

        var application = await GetApplicationInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (application is null)
            return null;

        redirectUrls = redirectUrls.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var applicationPatch = new Application()
        {
            Web = new WebApplication()
            {
                RedirectUris = redirectUrls
            }
        };
        
        await graphClient.Applications[application.Id]
            .Request()
            .UpdateAsync(applicationPatch)
            .ConfigureAwait(false);

        var servicePrincipal = await GetServicePrincipalInternalAsync(graphClient, identifier)
            .ConfigureAwait(false);

        if (servicePrincipal is not null)
        {
            var servicePrincipalPath = new ServicePrincipal()
            {
                ReplyUrls = redirectUrls
            };

            await graphClient.ServicePrincipals[servicePrincipal.Id]
                .Request()
                .UpdateAsync(servicePrincipalPath)
                .ConfigureAwait(false);
        }

        return redirectUrls;
    }

    public static async Task<List<Domain>> GetDomainsAsync(GraphServiceClient client)
    {
        try
        {
            var page = await client
                .Domains
                .Request()
                .GetAsync()
                .ConfigureAwait(false);

            var domains = page.CurrentPage.ToList();

            while (page.NextPageRequest is not null)
            {
                page = await page
                    .NextPageRequest
                    .GetAsync()
                    .ConfigureAwait(false);

                domains.AddRange(page.CurrentPage);
            }

            return domains;
        }
        // catch (ServiceException exc)
        catch (ServiceException)
        {
            return null;
        }
    }

    private static async Task<User> GetUserInternalAsync(GraphServiceClient client, string identifier)
    {
        if (identifier.StartsWithHttp())
            return null;

        if (!(identifier.IsGuid() || identifier.IsEMail()))
            return null;

        if (identifier.IsEMail())
        {
            var domains = await GetDomainsAsync(client);

            var hasVerifiedDomain = domains
                .Where(d => d.IsVerified.HasValue && d.IsVerified.Value)
                .Any(d => identifier.EndsWith($"@{d.Id}", StringComparison.OrdinalIgnoreCase));

            if (!hasVerifiedDomain)
            {
                var defaultDomain = domains
                    .First(d => d.IsDefault.HasValue && d.IsDefault.Value);

                identifier = $"{identifier.Replace("@", "_", StringComparison.OrdinalIgnoreCase)}#EXT#@{defaultDomain.Id}";
            }
        }

        try
        {
            return await client
                .Users[identifier]
                .Request()
                .Select(userSelect)
                .GetAsync()
                .ConfigureAwait(false);
        }
        catch (ServiceException)
        {
            return null;
        }
    }

    private static async Task<ServicePrincipal> GetServicePrincipalInternalAsync(GraphServiceClient client, string identifier)
    {
        var filter = $"servicePrincipalNames/any(p:p eq '{identifier}') or servicePrincipalNames/any(p:p eq 'api://{identifier}')";

        if (identifier.IsGuid())
            filter += $" or id eq '{identifier}'";

        try
        {
            var page = await client
                .ServicePrincipals
                .Request()
                .Filter(filter)
                .Select(servicePrincipalSelect)
                .GetAsync()
                .ConfigureAwait(false);

            var principal = page.CurrentPage.FirstOrDefault();

            while (principal is null && page.NextPageRequest is not null)
            {
                page = await page
                    .NextPageRequest
                    .GetAsync()
                    .ConfigureAwait(false);

                principal = page.CurrentPage.FirstOrDefault();
            }

            if (principal is not null)
                principal = await client.ServicePrincipals[principal.Id].Request().GetAsync().ConfigureAwait(false);

            return principal;
        }
        catch (ServiceException)
        {
            return null;
        }
    }

    private static async Task<Application> GetApplicationInternalAsync(GraphServiceClient client, string identifier)
    {
        var filter = $"identifierUris/any(p:p eq 'http://{identifier}') or identifierUris/any(p:p eq 'https://{identifier}') or identifierUris/any(p:p eq 'api://{identifier}')";

        if (identifier.IsGuid())
            filter += $" or id eq '{identifier}' or appId eq '{identifier}'";

        try
        {
            var page = await client
                .Applications
                .Request()
                .Filter(filter)
                .Select(applicationSelect)
                .GetAsync()
                .ConfigureAwait(false);

            var application = page.CurrentPage.FirstOrDefault();

            while (application is null && page.NextPageRequest is not null)
            {
                page = await page
                    .NextPageRequest
                    .GetAsync()
                    .ConfigureAwait(false);

                application = page.CurrentPage.FirstOrDefault();
            }

            if (application is not null)
                application = await client.Applications[application.Id].Request().GetAsync().ConfigureAwait(false);

            return application;
        }
        catch (ServiceException)
        {
            return null;
        }
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
}
