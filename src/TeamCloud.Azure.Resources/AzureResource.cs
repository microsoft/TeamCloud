/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.OData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Http;

namespace TeamCloud.Azure.Resources
{
    public class AzureResource
    {
        internal static async Task<T> InitializeAsync<T>(T azureResource, IAzureResourceService azureResourceService, bool throwIfNotExists)
            where T : AzureResource
        {
            azureResource.AzureResourceService = azureResourceService;

            var exists = await azureResource
                .ExistsAsync()
                .ConfigureAwait(false);

            if (exists)
                return azureResource;

            if (throwIfNotExists)
                throw new AzureResourceException($"Resource {azureResource.ResourceId} not found");

            return null;
        }

        internal AzureResource(string resourceId)
        {
            if (resourceId is null)
                throw new ArgumentNullException(nameof(resourceId));

            ResourceId = AzureResourceIdentifier.Parse(resourceId);
            AzureResourceService = null;
        }

        protected AzureResource(string resourceId, IAzureResourceService azureResourceService)
        {
            if (resourceId is null)
                throw new ArgumentNullException(nameof(resourceId));

            ResourceId = AzureResourceIdentifier.Parse(resourceId);
            AzureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        protected IAzureResourceService AzureResourceService { get; private set; }

        public AzureResourceIdentifier ResourceId { get; }

        public virtual async Task<bool> ExistsAsync()
        {
            var apiVersions = await AzureResourceService
                .GetApiVersionsAsync(ResourceId, true)
                .ConfigureAwait(false);

            var token = await AzureResourceService.AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var response = await ResourceId.GetApiUrl(AzureResourceService.AzureSessionService.Environment)
                .SetQueryParam("api-version", apiVersions.First())
                .WithOAuthBearerToken(token)
                .AllowAnyHttpStatus()
                .GetAsync(completionOption: HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }

        public virtual async IAsyncEnumerable<IAzureIdentity> GetIdentitiesAsync()
        {
            var json = await GetJsonAsync()
                .ConfigureAwait(false);

            var identity = json.SelectToken("$.identity");

            if (identity != null)
            {
                if (identity.SelectToken("type")?.ToString().Equals("SystemAssigned", StringComparison.OrdinalIgnoreCase) ?? false)
                    yield return identity.ToObject<AzureIdentity>();

                var userAssignedIdentities = identity
                    .SelectToken("userAssignedIdentities")?
                    .Children<JProperty>().Select(prop => prop.Name);

                foreach (var userAssignedIdentity in userAssignedIdentities)
                {
                    var identityResource = await AzureResourceService
                        .GetResourceAsync<AzureIdentityResource>(userAssignedIdentity, false)
                        .ConfigureAwait(false);

                    await foreach (var uaIdentity in identityResource?.GetIdentitiesAsync())
                        yield return uaIdentity;
                }
            }
        }

        public virtual async Task DeleteAsync(bool deleteLocks = false)
        {
            if (deleteLocks)
            {
                await DeleteLocksAsync(true)
                    .ConfigureAwait(false);
            }

            var apiVersions = await AzureResourceService
                .GetApiVersionsAsync(ResourceId, true)
                .ConfigureAwait(false);

            var token = await AzureResourceService.AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            await ResourceId.GetApiUrl(AzureResourceService.AzureSessionService.Environment)
                .SetQueryParam("api-version", apiVersions.First())
                .WithOAuthBearerToken(token)
                .AllowHttpStatus(HttpStatusCode.NotFound)
                .DeleteAsync()
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteLocksAsync(bool waitForDeletion = false)
        {
            var locks = await GetLocksInternalAsync()
                .ConfigureAwait(false);

            if (locks.Any())
            {
                var session = await AzureResourceService.AzureSessionService
                    .CreateSessionAsync(this.ResourceId.SubscriptionId)
                    .ConfigureAwait(false);

                await session.ManagementLocks
                    .DeleteByIdsAsync(locks.ToArray())
                    .ConfigureAwait(false);

                if (waitForDeletion)
                {
                    var timeoutDuration = TimeSpan.FromMinutes(5);
                    var timeout = DateTime.UtcNow.Add(timeoutDuration);

                    while (DateTime.UtcNow < timeout && locks.Any())
                    {
                        await Task.Delay(5000)
                            .ConfigureAwait(false);

                        locks = await GetLocksInternalAsync()
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<IEnumerable<string>> GetLocksInternalAsync()
        {
            var session = await AzureResourceService.AzureSessionService
                .CreateSessionAsync(this.ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            var locks = new List<string>();

            var page = await session.ManagementLocks
                .ListForResourceAsync(this.ResourceId.ToString())
                .ConfigureAwait(false);

            while (page?.Any() ?? false)
            {
                locks.AddRange(page.Select(lck => lck.Id));

                page = await page.GetNextPageAsync()
                    .ConfigureAwait(false);
            }

            return locks.Distinct();
        }

        protected virtual async Task<string> GetLatestApiVersionAsync(bool includePreviewVersions = false)
        {
            var apiVersions = await AzureResourceService
                .GetApiVersionsAsync(this.ResourceId, includePreviewVersions)
                .ConfigureAwait(false);

            return apiVersions.FirstOrDefault();
        }

        public async Task<string> GetLocationAsync()
        {
            var json = await GetJsonAsync().ConfigureAwait(false);

            return json?.SelectToken("location")?.ToString();
        }

        public async Task<JObject> GetJsonAsync(string apiVersion = null)
        {
            apiVersion ??= await GetLatestApiVersionAsync()
                .ConfigureAwait(false);

            var token = await AzureResourceService.AzureSessionService
                .AcquireTokenAsync(AzureEndpoint.ResourceManagerEndpoint)
                .ConfigureAwait(false);

            return await ResourceId.GetApiUrl(AzureResourceService.AzureSessionService.Environment)
                .SetQueryParam("api-version", apiVersion)
                .WithOAuthBearerToken(token)
                .GetJObjectAsync()
                .ConfigureAwait(false);
        }

        private async Task<GenericResourceInner> GetResourceAsync()
        {
            using var resourceManagementClient = await AzureResourceService.AzureSessionService
                .CreateClientAsync<ResourceManagementClient>(subscriptionId: this.ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            var apiVersion = await GetLatestApiVersionAsync()
                .ConfigureAwait(false);

            return await resourceManagementClient.Resources
                .GetByIdAsync(this.ResourceId.ToString(), apiVersion)
                .ConfigureAwait(false);
        }

        private async Task<GenericResourceInner> SetResourceAsync(GenericResourceInner resource)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            using var resourceManagementClient = await AzureResourceService.AzureSessionService
                .CreateClientAsync<ResourceManagementClient>(subscriptionId: this.ResourceId.SubscriptionId)
                .ConfigureAwait(false);

            var apiVersion = await GetLatestApiVersionAsync()
                .ConfigureAwait(false);

            if (resource.Properties != null)
            {
                // we need to ensure that the update call doesn't contain
                // a provisioning state information - otherwise the update
                // call will end up in a BAD REQUEST error

                var properties = JObject.FromObject(resource.Properties);
                properties.SelectToken("$.provisioningState")?.Parent?.Remove();
                resource.Properties = properties;
            }

            return await resourceManagementClient.Resources
                .UpdateByIdAsync(this.ResourceId.ToString(), apiVersion, resource)
                .ConfigureAwait(false);
        }

        public virtual async Task<IDictionary<string, string>> GetTagsAsync(bool includeHidden = false)
        {
            var resource = await GetResourceAsync()
                .ConfigureAwait(false);

            var resourceTags = resource.Tags ?? new Dictionary<string, string>();

            if (includeHidden)
                return resourceTags;

            return resourceTags
                .Where(kvp => !kvp.Key.StartsWith("hidden-", StringComparison.OrdinalIgnoreCase))
                .ToDictionary();
        }

        public virtual async Task SetTagsAsync(IDictionary<string, string> tags, bool merge = false)
        {
            var resource = await GetResourceAsync()
                .ConfigureAwait(false);

            resource.Tags ??= new Dictionary<string, string>();

            if (tags?.Any() ?? false)
            {
                if (merge)
                {
                    resource.Tags = resource.Tags
                        .Override(tags)
                        .Where(kvp => kvp.Value != null)
                        .ToDictionary();
                }
                else
                {
                    resource.Tags = tags
                        .Where(kvp => kvp.Value != null)
                        .ToDictionary();
                }
            }
            else
            {
                if (merge) return;

                resource.Tags.Clear();
            }

            _ = await SetResourceAsync(resource)
                .ConfigureAwait(false);
        }

        public async Task<string> GetTagAsync(string key)
        {
            var tags = await GetTagsAsync(true)
                .ConfigureAwait(false);

            if (tags.TryGetValue(key, out string value))
                return value;

            return default;
        }

        public async Task SetTagAsync(string key, string value = default)
        {
            var tags = await GetTagsAsync(true)
                .ConfigureAwait(false);

            if (tags.TryGetValue(key, out string currentValue) && currentValue == value)
            {
                return; // no need to update or delete
            }
            else
            {
                tags[key] = value;
            }

            await SetTagsAsync(tags)
                .ConfigureAwait(false);
        }

        public virtual async Task AddRoleAssignmentAsync(string userObjectId, Guid roleDefinitionId)
        {
            if (string.IsNullOrEmpty(userObjectId))
                throw new ArgumentException($"'{nameof(userObjectId)}' cannot be null or empty.", nameof(userObjectId));

            using var authClient = await AzureResourceService.AzureSessionService
                .CreateClientAsync<AuthorizationManagementClient>()
                .ConfigureAwait(false);

            var parameters = new RoleAssignmentCreateParameters
            {
                PrincipalId = userObjectId,
                RoleDefinitionId = $"/subscriptions/{ResourceId.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleDefinitionId}"
            };

            try
            {
                _ = await authClient
                    .RoleAssignments
                    .CreateAsync(ResourceId.ToString().TrimStart('/'), Guid.NewGuid().ToString(), parameters)
                    .ConfigureAwait(false);
            }
            catch (CloudException exc) when (exc.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // swallow this exception as the role assignment already exists
            }
        }

        public virtual async Task DeleteRoleAssignmentAsync(string userObjectId, Guid? roleDefinitionId = null)
        {
            if (string.IsNullOrEmpty(userObjectId))
                throw new ArgumentException($"'{nameof(userObjectId)}' cannot be null or empty.", nameof(userObjectId));

            var assignments = await GetRoleAssignmentsInternalAsync(userObjectId, false)
                .ConfigureAwait(false);

            if (assignments.TryGetValue(userObjectId, out var roleAssignments))
            {
                using var authClient = await AzureResourceService.AzureSessionService
                    .CreateClientAsync<AuthorizationManagementClient>()
                    .ConfigureAwait(false);

                var deleteTasks = roleAssignments
                    .Where(roleAssignment => !roleDefinitionId.HasValue || roleAssignment.GetRoleDefinitionId().Equals(roleDefinitionId.Value))
                    .Select(roleAssignment => authClient.RoleAssignments.DeleteByIdAsync(roleAssignment.Id));

                await Task
                    .WhenAll(deleteTasks)
                    .ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> HasRoleAssignmentAsync(string userObjectId, Guid roleDefinitionId, bool includeInherited = false)
        {
            if (string.IsNullOrEmpty(userObjectId))
                throw new ArgumentException($"'{nameof(userObjectId)}' cannot be null or empty.", nameof(userObjectId));

            var assignments = await GetRoleAssignmentsInternalAsync(userObjectId, includeInherited)
                .ConfigureAwait(false);

            return assignments
                .SelectMany(kvp => kvp.Value)
                .Any(assignment => assignment.GetRoleDefinitionId() == roleDefinitionId);
        }

        public virtual async Task<IEnumerable<Guid>> GetRoleAssignmentsAsync(string userObjectId)
        {
            if (string.IsNullOrEmpty(userObjectId))
                throw new ArgumentException($"'{nameof(userObjectId)}' cannot be null or empty.", nameof(userObjectId));

            var assignments = await GetRoleAssignmentsInternalAsync(userObjectId, false)
                .ConfigureAwait(false);

            if (assignments.TryGetValue(userObjectId, out var roleAssignments))
                return roleAssignments.Select(roleAssignment => roleAssignment.GetRoleDefinitionId());

            return Enumerable.Empty<Guid>();
        }

        public virtual async Task SetRoleAssignmentsAsync(IDictionary<string, IEnumerable<Guid>> roleAssignments)
        {
            if (roleAssignments is null)
                throw new ArgumentNullException(nameof(roleAssignments));

            var assignments = await GetRoleAssignmentsAsync()
                .ConfigureAwait(false);

            var tasks = new List<Task>();

            // delete all role assignments
            // for users that don't exist
            // in our target state

            tasks.AddRange(assignments.Keys
                .Except(roleAssignments.Keys)
                .Select(userObjectId => DeleteRoleAssignmentAsync(userObjectId)));

            // add all role assignments
            // for users that don't exist
            // in our current state

            tasks.AddRange(roleAssignments.Keys
                .Except(assignments.Keys)
                .SelectMany(userObjectId => roleAssignments[userObjectId].Select(roleId => AddRoleAssignmentAsync(userObjectId, roleId))));

            // update role assignments
            // for users existing in
            // our current and target state

            foreach (var userObjectId in assignments.Keys.Intersect(roleAssignments.Keys))
            {
                var currentRoleIds = assignments[userObjectId];
                var targetRoleIds = roleAssignments[userObjectId];

                tasks.AddRange(currentRoleIds
                    .Except(targetRoleIds)
                    .Select(roleId => DeleteRoleAssignmentAsync(userObjectId, roleId)));

                tasks.AddRange(targetRoleIds
                    .Except(currentRoleIds)
                    .Select(roleId => AddRoleAssignmentAsync(userObjectId, roleId)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        public virtual async Task<IDictionary<string, IEnumerable<Guid>>> GetRoleAssignmentsAsync(bool includeInherited = false)
        {
            var assignments = await GetRoleAssignmentsInternalAsync(null, includeInherited)
                .ConfigureAwait(false);

            return assignments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(assignment => assignment.GetRoleDefinitionId()));
        }

        private async Task<IDictionary<string, IEnumerable<RoleAssignmentInner>>> GetRoleAssignmentsInternalAsync(string userObjectId, bool includeInherited)
        {
            using var authClient = await AzureResourceService.AzureSessionService
                .CreateClientAsync<AuthorizationManagementClient>()
                .ConfigureAwait(false);

            var roles = new List<RoleAssignmentInner>();
            var query = new ODataQuery<RoleAssignmentFilter>();

            if (!string.IsNullOrEmpty(userObjectId))
                query.SetFilter(filter => filter.PrincipalId == userObjectId);

            var page = await authClient.RoleAssignments
                .ListForScopeAsync(ResourceId.ToString().TrimStart('/'), query)
                .ConfigureAwait(false);

            roles.AddRange(page);

            while (!string.IsNullOrEmpty(page.NextPageLink))
            {
                page = await authClient.RoleAssignments
                    .ListForScopeNextAsync(page.NextPageLink)
                    .ConfigureAwait(false);

                roles.AddRange(page);
            }

            return roles
                .Where(role => includeInherited || role.Scope.Equals(ResourceId.ToString(), StringComparison.OrdinalIgnoreCase))
                .GroupBy(role => role.PrincipalId)
                .ToDictionary(grp => grp.Key, grp => grp.AsEnumerable());
        }
    }
}
