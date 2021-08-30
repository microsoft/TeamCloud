/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Validation;
using User = TeamCloud.Model.Data.User;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbUserRepository : CosmosDbRepository<User>, IUserRepository
    {
        public CosmosDbUserRepository(ICosmosDbOptions options,
                                      IDocumentExpanderProvider expanderProvider = null,
                                      IDocumentSubscriptionProvider subscriptionProvider = null,
                                      IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanderProvider, subscriptionProvider, dataProtectionProvider)
        { }

        public override async Task<User> AddAsync(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            await user
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(user, GetPartitionKey(user))
                .ConfigureAwait(false);

            user = await ExpandAsync(response.Resource)
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(user, DocumentSubscriptionEvent.Create)
                .ConfigureAwait(false);
        }

        public override async Task<User> GetAsync(string organization, string id, bool expand = false)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<User>(id, GetPartitionKey(organization))
                    .ConfigureAwait(false);

                return await ExpandAsync(response.Resource, expand)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task<User> GetAsync(User user, bool expand = false)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<User>(user.Id, GetPartitionKey(user))
                    .ConfigureAwait(false);

                return await ExpandAsync(response.Resource, expand)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public IAsyncEnumerable<string> ListOrgsAsync(User user)
            => ListOrgsAsync(user?.Id ?? throw new ArgumentNullException(nameof(user)));

        public async IAsyncEnumerable<string> ListOrgsAsync(string userId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM u WHERE u.id = '{userId}'");

            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: new QueryRequestOptions { MaxBufferedItemCount = -1, MaxConcurrency = -1 });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var user in queryResponse)
                    yield return user.Organization;
            }
        }

        public override async IAsyncEnumerable<User> ListAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM u");

            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var user in queryResponse)
                    yield return await ExpandAsync(user).ConfigureAwait(false);
            }
        }

        public async IAsyncEnumerable<User> ListAsync(string organization, string projectId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT VALUE u FROM u WHERE EXISTS(SELECT VALUE m FROM m IN u.projectMemberships WHERE m.projectId = '{projectId}')");

            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var user in queryResponse)
                    yield return await ExpandAsync(user).ConfigureAwait(false);
            }
        }

        public async IAsyncEnumerable<User> ListOwnersAsync(string organization, string projectId = null)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = string.IsNullOrWhiteSpace(projectId)
                ? new QueryDefinition($"SELECT * FROM u WHERE u.role = '{OrganizationUserRole.Owner}'")
                : new QueryDefinition($"SELECT VALUE u FROM u WHERE EXISTS(SELECT VALUE m FROM m IN u.projectMemberships WHERE m.projectId = '{projectId}' AND m.role = '{ProjectUserRole.Owner}')");

            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var user in queryResponse)
                    yield return await ExpandAsync(user).ConfigureAwait(false);
            }
        }

        public async IAsyncEnumerable<User> ListAdminsAsync(string organization, string projectId = null)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = string.IsNullOrWhiteSpace(projectId)
                ? new QueryDefinition($"SELECT * FROM u WHERE u.role = '{OrganizationUserRole.Admin}'")
                : new QueryDefinition($"SELECT VALUE u FROM u WHERE EXISTS(SELECT VALUE m FROM m IN u.projectMemberships WHERE m.projectId = '{projectId}' AND m.role = '{ProjectUserRole.Admin}')");

            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var user in queryResponse)
                    yield return await ExpandAsync(user).ConfigureAwait(false);
            }
        }

        public override async Task<User> SetAsync(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            await user
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(user, GetPartitionKey(user))
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
                .ConfigureAwait(false);
        }

        public override async Task<User> RemoveAsync(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<User>(user.Id, GetPartitionKey(user))
                    .ConfigureAwait(false);

                return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public async Task RemoveProjectMembershipsAsync(string organization, string projectId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT VALUE u FROM u WHERE EXISTS(SELECT VALUE m FROM m IN u.projectMemberships WHERE m.projectId = '{projectId}')");
            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: GetQueryRequestOptions(organization));

            var tasks = new List<Task>();

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                tasks.AddRange(queryResponse.Select(user => RemoveProjectMembershipAsync(user, projectId)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task<User> RemoveProjectMembershipAsync(User user, string projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(((IContainerDocument)user).ETag))
            {
                var existingUser = await GetAsync(user)
                    .ConfigureAwait(false);

                // user no longer exists
                if (existingUser is null)
                    return null;

                user = existingUser;
            }

            return await RemoveProjectMembershipSafeAsync(container, user, projectId)
                .ConfigureAwait(false);

            async Task<User> RemoveProjectMembershipSafeAsync(Container container, User user, string projectId)
            {
                var membership = user.ProjectMemberships.FirstOrDefault(m => m.ProjectId == projectId);

                if (membership is null)
                    return user;

                while (true)
                {
                    try
                    {
                        user.ProjectMemberships.Remove(membership);

                        var response = await container
                            .ReplaceItemAsync(user, user.Id, GetPartitionKey(user), new ItemRequestOptions { IfMatchEtag = ((IContainerDocument)user).ETag })
                            .ConfigureAwait(false);

                        return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
                            .ConfigureAwait(false);
                    }
                    catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
                    {
                        // the requested user does not exist anymore - continue
                        return null;
                    }
                    catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        // the requested user has changed, get it again before proceeding
                        user = await GetAsync(user)
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        public Task<User> AddProjectMembershipAsync(User user, string projectId, ProjectUserRole role, IDictionary<string, string> properties)
        {
            return AddProjectMembershipAsync(user, new ProjectMembership
            {
                ProjectId = projectId,
                Role = role,
                Properties = properties ?? new Dictionary<string, string>()
            });
        }

        // this method can only change project memberships
        // other changes to the user object will be overwitten
        public async Task<User> AddProjectMembershipAsync(User user, ProjectMembership membership)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (membership is null)
                throw new ArgumentNullException(nameof(membership));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(((IContainerDocument)user).ETag))
            {
                var existingUser = await GetAsync(user)
                    .ConfigureAwait(false)
                ?? await AddAsync(user)
                    .ConfigureAwait(false);

                user = existingUser;
            }

            return await AddProjectMembershipSafeAsync(container, user, membership)
                .ConfigureAwait(false);

            async Task<User> AddProjectMembershipSafeAsync(Container container, User user, ProjectMembership membership)
            {
                while (true)
                {
                    try
                    {
                        user.EnsureProjectMembership(membership);

                        var response = await container
                            .ReplaceItemAsync(user, user.Id, GetPartitionKey(user), new ItemRequestOptions { IfMatchEtag = ((IContainerDocument)user).ETag })
                            .ConfigureAwait(false);

                        return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
                            .ConfigureAwait(false);
                    }
                    catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
                    {
                        // the requested user does not exist anymore - continue
                        return null;
                    }
                    catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        // the requested user has changed, get it again before proceeding
                        user = await GetAsync(user)
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        public async Task<User> SetOrganizationInfoAsync(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(((IContainerDocument)user).ETag))
            {
                var existingUser = await GetAsync(user)
                    .ConfigureAwait(false)
                ?? await AddAsync(user)
                    .ConfigureAwait(false);

                user = existingUser;
            }

            return await SetOrganizationInfoAsync(container, user)
                .ConfigureAwait(false);


            async Task<User> SetOrganizationInfoAsync(Container container, User user)
            {
                while (true)
                {
                    try
                    {
                        var response = await container
                            .ReplaceItemAsync(user, user.Id, GetPartitionKey(user), new ItemRequestOptions { IfMatchEtag = ((IContainerDocument)user).ETag })
                            .ConfigureAwait(false);

                        return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
                            .ConfigureAwait(false);
                    }
                    catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
                    {
                        // the requested user does not exist anymore - continue
                        return null;
                    }
                    catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        // the requested user has changed, get it again before proceeding
                        user = await GetAsync(user)
                            .ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
