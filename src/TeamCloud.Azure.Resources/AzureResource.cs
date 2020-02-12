using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.OData;

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

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task AddRoleAssignmentAsync(Guid userObjectId, Guid roleDefinitionId)
        {
            using var authClient = AzureResourceService.AzureSessionService
                .CreateClient<AuthorizationManagementClient>();

            var parameters = new RoleAssignmentCreateParameters
            {
                PrincipalId = userObjectId.ToString(),
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

        public async Task DeleteRoleAssignmentAsync(Guid userObjectId, Guid? roleDefinitionId = null)
        {
            var assignments = await GetRoleAssignmentsInternalAsync(userObjectId)
                .ConfigureAwait(false);

            if (assignments.TryGetValue(userObjectId, out var roleAssignments))
            {
                using var authClient = AzureResourceService.AzureSessionService
                    .CreateClient<AuthorizationManagementClient>();

                var deleteTasks = roleAssignments
                    .Where(roleAssignment => !roleDefinitionId.HasValue || roleAssignment.GetRoleDefinitionId().Equals(roleDefinitionId.Value))
                    .Select(roleAssignment => authClient.RoleAssignments.DeleteByIdAsync(roleAssignment.Id));

                await Task
                    .WhenAll(deleteTasks)
                    .ConfigureAwait(false);
            }
        }

        public async Task<bool> HasRoleAssignmentAsync(Guid userObjectId, Guid roleDefinitionId)
        {
            var assignments = await GetRoleAssignmentsAsync(userObjectId)
                .ConfigureAwait(false);

            return assignments.Contains(roleDefinitionId);
        }

        public async Task<IEnumerable<Guid>> GetRoleAssignmentsAsync(Guid userObjectId)
        {
            var assignments = await GetRoleAssignmentsInternalAsync(userObjectId)
                .ConfigureAwait(false);

            if (assignments.TryGetValue(userObjectId, out var roleAssignments))
                return roleAssignments.Select(roleAssignment => roleAssignment.GetRoleDefinitionId());

            return Enumerable.Empty<Guid>();
        }

        public async Task<IReadOnlyDictionary<Guid, IEnumerable<Guid>>> GetRoleAssignmentsAsync()
        {
            var assignments = await GetRoleAssignmentsInternalAsync()
                .ConfigureAwait(false);

            return assignments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(assignment => assignment.GetRoleDefinitionId()));
        }

        private async Task<IReadOnlyDictionary<Guid, IEnumerable<RoleAssignmentInner>>> GetRoleAssignmentsInternalAsync(Guid? userObjectId = null)
        {
            using var authClient = AzureResourceService.AzureSessionService
                .CreateClient<AuthorizationManagementClient>();

            var roles = new List<RoleAssignmentInner>();
            var query = new ODataQuery<RoleAssignmentFilter>();

            if (userObjectId.HasValue)
                query.SetFilter(filter => filter.PrincipalId.Equals(userObjectId.Value.ToString(), StringComparison.OrdinalIgnoreCase));

            var page = await authClient.RoleAssignments
                .ListForScopeAsync(ResourceId.ToString().TrimStart('/'))
                .ConfigureAwait(false);

            roles.AddRange(page);

            while (!string.IsNullOrEmpty(page.NextPageLink))
            {
                page = await authClient.RoleAssignments
                    .ListForScopeNextAsync(page.NextPageLink)
                    .ConfigureAwait(false);

                roles.AddRange(page);
            }

            // ListForScopeAsync includes scopes which encompass the resource, e.g. it returns
            // assignments inherited from the subscription. We filter those out here because we can't
            // modify them.

            return roles
                .Where(role => role.Scope.Equals(ResourceId.ToString(), StringComparison.OrdinalIgnoreCase))
                .GroupBy(role => Guid.Parse(role.PrincipalId))
                .ToDictionary(grp => grp.Key, grp => grp.AsEnumerable());
        }
    }
}
