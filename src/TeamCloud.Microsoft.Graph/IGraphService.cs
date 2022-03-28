/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamCloud.Microsoft.Graph;

public interface IGraphService
{
    Task<bool> IsUserAsync(string identifier);

    Task<string> GetUserIdAsync(string identifier);

    Task<bool> IsGroupAsync(string identifier);

    Task<string> GetGroupIdAsync(string identifier);

    IAsyncEnumerable<string> GetGroupMembersAsync(string identifier, bool resolveAllGroups = false);

    Task<string> GetDisplayNameAsync(string identifier);

    Task<string> GetLoginNameAsync(string identifier);

    Task<string> GetMailAddressAsync(string identifier);

    Task<AzureServicePrincipal> CreateServicePrincipalAsync(string name);

    Task<AzureServicePrincipal> RefreshServicePrincipalAsync(string identifier);

    Task<AzureServicePrincipal> GetServicePrincipalAsync(string identifier);

    Task DeleteServicePrincipalAsync(string identifier);

    Task<IEnumerable<string>> GetServicePrincipalRedirectUrlsAsync(string identifier);

    Task<IEnumerable<string>> SetServicePrincipalRedirectUrlsAsync(string identifier, params string[] redirectUrls);

    Task<Dictionary<string, Dictionary<Guid, AccessType>>> GetResourcePermissionsAsync(string identifier);

    Task<Dictionary<string, Dictionary<Guid, AccessType>>> SetResourcePermissionsAsync(string identifier, Dictionary<string, Dictionary<Guid, AccessType>> resourcePermissions);
}
