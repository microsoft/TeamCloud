using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public sealed class CosmosDbProjectIdentityRepository : CosmosDbRepository<ProjectIdentity>, IProjectIdentityRepository
    {
        public CosmosDbProjectIdentityRepository(ICosmosDbOptions options, IEnumerable<IDocumentExpander> expanders)
            : base(options, expanders)
        { }

        public override async Task<ProjectIdentity> AddAsync(ProjectIdentity document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            _ = await document
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .CreateItemAsync(document)
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a project with name)
            }
        }

        public override async Task<ProjectIdentity> GetAsync(string projectId, string identifier, bool expand = false)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            if (!Guid.TryParse(identifier, out var identifierParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(identifier));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProjectIdentity>(identifierParsed.ToString(), GetPartitionKey(projectId))
                    .ConfigureAwait(false);

                var expandTask = expand
                    ? ExpandAsync(response.Resource)
                    : Task.FromResult(response.Resource);

                return await expandTask.ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override async IAsyncEnumerable<ProjectIdentity> ListAsync(string projectId)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var queryString = $"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}'";

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ProjectIdentity>(query, requestOptions: GetQueryRequestOptions(projectId));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public override async Task<ProjectIdentity> RemoveAsync(ProjectIdentity document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ProjectIdentity>(document.Id, GetPartitionKey(document))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public override async Task<ProjectIdentity> SetAsync(ProjectIdentity document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            await document
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(document, GetPartitionKey(document))
                .ConfigureAwait(false);

            return response.Resource;
        }
    }
}
