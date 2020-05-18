/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Cosmos;
using TeamCloud.Model;
using TeamCloud.Model.Data;

using User = TeamCloud.Model.Data.User;

namespace TeamCloud.Data.CosmosDb
{

    public class CosmosDbUsersRepository : CosmosDbBaseRepository, IUsersRepository
    {
        public CosmosDbUsersRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<User> AddAsync(User user)
        {
            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(user, new PartitionKey(Constants.CosmosDb.TenantName))
                .ConfigureAwait(false);

            return response.Value;
        }

        public async Task<User> GetAsync(Guid id)
        {
            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<User>(id.ToString(), new PartitionKey(Constants.CosmosDb.TenantName))
                    .ConfigureAwait(false);

                return response.Value;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async IAsyncEnumerable<User> ListAsync()
        {
            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM u");
            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TenantName) });

            await foreach (var user in queryIterator)
                yield return user;
        }

        public async IAsyncEnumerable<User> ListAsync(Guid projectId)
        {
            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var query = new QueryDefinition("SELECT VALUE u FROM u WHERE EXISTS(SELECT VALUE m FROM m IN u.projectMemberships WHERE m.projectId = @projectId)")
                .WithParameter("@projectId", projectId);

            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TenantName) });

            await foreach (var user in queryIterator)
                yield return user;
        }

        public async IAsyncEnumerable<User> ListOwnersAsync(Guid projectId)
        {
            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var query = new QueryDefinition("SELECT VALUE u FROM u WHERE EXISTS(SELECT VALUE m FROM m IN u.projectMemberships WHERE m.projectId = @projectId AND m.role = @projectRole)")
                .WithParameter("@projectId", projectId)
                .WithParameter("@projectRole", ProjectUserRole.Owner);

            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TenantName) });

            await foreach (var user in queryIterator)
                yield return user;
        }

        public async IAsyncEnumerable<User> ListAdminsAsync()
        {
            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM u WHERE u.role = @role")
                .WithParameter("@role", TeamCloudUserRole.Admin);
            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TenantName) });

            await foreach (var user in queryIterator)
                yield return user;
        }

        public async Task<User> SetAsync(User user)
        {
            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync<User>(user, new PartitionKey(Constants.CosmosDb.TenantName))
                .ConfigureAwait(false);

            return response.Value;
        }

        public async Task<User> RemoveAsync(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<User>(user.Id.ToString(), new PartitionKey(Constants.CosmosDb.TenantName))
                    .ConfigureAwait(false);

                return response.Value;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public async Task RemoveProjectMembershipsAsync(Guid projectId)
        {
            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var query = new QueryDefinition("SELECT VALUE u FROM u WHERE EXISTS(SELECT VALUE m FROM m IN u.projectMemberships WHERE m.projectId = @projectId)")
                .WithParameter("@projectId", projectId);

            var queryIterator = container.GetItemQueryIterator<User>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TenantName) });

            await foreach (var user in queryIterator)
                _ = await RemoveProjectMembershipAsync(user, projectId)
                    .ConfigureAwait(false);
        }

        public async Task<User> RemoveProjectMembershipAsync(User user, Guid projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var etag = user.ETag;

            if (string.IsNullOrEmpty(etag))
            {
                var existingUser = await GetAsync(user.Id)
                    .ConfigureAwait(false);

                // user no longer exists
                if (existingUser is null)
                    return null;

                user = existingUser;
            }

            return await RemoveProjectMembershipSafeAsync(container, user, projectId)
                .ConfigureAwait(false);


            async Task<User> RemoveProjectMembershipSafeAsync(Container container, User user, Guid projectId)
            {
                var membership = user.ProjectMemberships.FirstOrDefault(m => m.ProjectId == projectId);

                if (membership is null)
                    return user;

                user.ProjectMemberships.Remove(membership);

                try
                {
                    var updatedUser = await container
                        .ReplaceItemAsync(
                            user, user.Id.ToString(),
                            new PartitionKey(Constants.CosmosDb.TenantName),
                            new ItemRequestOptions { IfMatchEtag = user.ETag }
                        ).ConfigureAwait(false);

                    return updatedUser;
                }
                catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // the requested user does not exist anymore - continue
                    // return user;
                }
                catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    // the requested user has changed, get it again before proceeding
                    var refreshedUser = await GetAsync(user.Id)
                        .ConfigureAwait(false);

                    // TODO: add some safety here to prevent infinate recursion
                    return await RemoveProjectMembershipSafeAsync(container, refreshedUser, projectId)
                        .ConfigureAwait(false);
                }
            }
        }

        public Task<User> AddProjectMembershipAsync(User user, Guid projectId, ProjectUserRole role, IDictionary<string, string> properties)
            => AddProjectMembershipAsync(user, new ProjectMembership
            {
                ProjectId = projectId,
                Role = role,
                Properties = properties ?? new Dictionary<string, string>()
            });

        // this method can only change project memberships
        // other changes to the user object will be overwitten
        public async Task<User> AddProjectMembershipAsync(User user, ProjectMembership membership)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var container = await GetContainerAsync<User>()
                .ConfigureAwait(false);

            var etag = user.ETag;

            if (string.IsNullOrEmpty(etag))
            {
                var existingUser = await GetAsync(user.Id)
                    .ConfigureAwait(false);

                // user doesn't exist yet, create it
                if (existingUser is null)
                    existingUser = await AddAsync(user)
                        .ConfigureAwait(false);

                user = existingUser;
            }

            return await AddProjectMembershipSafeAsync(container, user, membership)
                .ConfigureAwait(false);


            async Task<User> AddProjectMembershipSafeAsync(Container container, User user, ProjectMembership membership)
            {
                user.EnsureProjectMembership(membership);

                try
                {
                    var updatedUser = await container
                        .ReplaceItemAsync(
                            user, user.Id.ToString(),
                            new PartitionKey(Constants.CosmosDb.TenantName),
                            new ItemRequestOptions { IfMatchEtag = user.ETag }
                        ).ConfigureAwait(false);

                    return updatedUser;
                }
                catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // the requested user does not exist anymore - continue
                    // return user;
                }
                catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    // the requested user has changed, get it again before proceeding
                    var refreshedUser = await GetAsync(user.Id)
                        .ConfigureAwait(false);

                    // TODO: add some safety here to prevent infinate recursion
                    return await AddProjectMembershipSafeAsync(container, refreshedUser, membership)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
