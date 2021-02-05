/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Threading.Tasks;
// using Microsoft.Azure.Cosmos;
// using TeamCloud.Data.CosmosDb.Core;
// using TeamCloud.Model.Data;
// using TeamCloud.Model.Validation;

// namespace TeamCloud.Data.CosmosDb
// {
//     public sealed class CosmosDbProjectLinkRepository : CosmosDbRepository<ProjectLink>, IProjectLinkRepository
//     {
//         public CosmosDbProjectLinkRepository(ICosmosDbOptions cosmosOptions)
//             : base(cosmosOptions)
//         { }

//         public async Task<ProjectLink> AddAsync(ProjectLink projectLink)
//         {
//             if (projectLink is null)
//                 throw new ArgumentNullException(nameof(projectLink));

//             await projectLink
//                 .ValidateAsync(throwOnValidationError: true)
//                 .ConfigureAwait(false);

//             var container = await GetContainerAsync()
//                 .ConfigureAwait(false);

//             var response = await container
//                 .CreateItemAsync(projectLink, GetPartitionKey(projectLink))
//                 .ConfigureAwait(false);

//             return response.Resource;
//         }

//         public async Task<ProjectLink> GetAsync(string projectId, string linkId)
//         {
//             if (projectId is null)
//                 throw new ArgumentNullException(nameof(projectId));

//             if (!Guid.TryParse(projectId, out var projectIdParsed))
//                 throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

//             if (linkId is null)
//                 throw new ArgumentNullException(nameof(linkId));

//             if (!Guid.TryParse(linkId, out var linkIdParsed))
//                 throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

//             var container = await GetContainerAsync()
//                 .ConfigureAwait(false);

//             try
//             {
//                 var response = await container
//                     .ReadItemAsync<ProjectLink>(linkIdParsed.ToString(), GetPartitionKey(projectIdParsed.ToString()))
//                     .ConfigureAwait(false);

//                 return response.Resource;
//             }
//             catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
//             {
//                 return null;
//             }
//         }

//         public async IAsyncEnumerable<ProjectLink> ListAsync(string projectId)
//         {
//             if (projectId is null)
//                 throw new ArgumentNullException(nameof(projectId));

//             if (!Guid.TryParse(projectId, out var projectIdParsed))
//                 throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

//             var container = await GetContainerAsync()
//                 .ConfigureAwait(false);

//             var query = new QueryDefinition($"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}'");

//             var queryIterator = container
//                 .GetItemQueryIterator<ProjectLink>(query, requestOptions: GetQueryRequestOptions(projectIdParsed.ToString()));

//             while (queryIterator.HasMoreResults)
//             {
//                 var queryResponse = await queryIterator
//                     .ReadNextAsync()
//                     .ConfigureAwait(false);

//                 foreach (var queryResult in queryResponse)
//                     yield return queryResult;
//             }
//         }

//         public async Task<ProjectLink> RemoveAsync(ProjectLink projectLink)
//         {
//             if (projectLink is null)
//                 throw new ArgumentNullException(nameof(projectLink));

//             var container = await GetContainerAsync()
//                 .ConfigureAwait(false);

//             try
//             {
//                 var response = await container
//                     .DeleteItemAsync<ProjectLink>(projectLink.Id, GetPartitionKey(projectLink))
//                     .ConfigureAwait(false);

//                 return response.Resource;
//             }
//             catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
//             {
//                 return null; // already deleted
//             }
//         }

//         public async Task RemoveAsync(string projectId)
//         {
//             var projectLinks = ListAsync(projectId);

//             if (await projectLinks.AnyAsync().ConfigureAwait(false))
//             {
//                 var container = await GetContainerAsync()
//                     .ConfigureAwait(false);

//                 var batch = container
//                     .CreateTransactionalBatch(GetPartitionKey(projectId));

//                 await foreach (var projectLink in projectLinks.ConfigureAwait(false))
//                     batch = batch.DeleteItem(projectLink.Id);

//                 await batch
//                     .ExecuteAsync()
//                     .ConfigureAwait(false);
//             }
//         }

//         public async Task RemoveAsync(string projectId, string linkId)
//         {
//             var projectLink = await GetAsync(projectId, linkId)
//                 .ConfigureAwait(false);

//             if (projectLink != null)
//             {
//                 await RemoveAsync(projectLink)
//                     .ConfigureAwait(false);
//             }
//         }

//         public async Task<ProjectLink> SetAsync(ProjectLink projectLink)
//         {
//             if (projectLink is null)
//                 throw new ArgumentNullException(nameof(projectLink));

//             await projectLink
//                 .ValidateAsync(throwOnValidationError: true)
//                 .ConfigureAwait(false);

//             var container = await GetContainerAsync()
//                 .ConfigureAwait(false);

//             var response = await container
//                 .UpsertItemAsync(projectLink, GetPartitionKey(projectLink))
//                 .ConfigureAwait(false);

//             return response.Resource;
//         }
//     }
// }
