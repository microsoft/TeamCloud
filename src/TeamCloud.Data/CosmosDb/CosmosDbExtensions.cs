/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data.CosmosDb
{
    internal static class CosmosDbExtensions
    {
        internal static IEnumerable<T> GetOperationResultResources<T>(this TransactionalBatchResponse batchResponse)
        {
            if (!batchResponse.IsSuccessStatusCode)
                yield break;

            for (int i = 0; i < batchResponse.Count; i++)
            {
                var operationResult = batchResponse.GetOperationResultAtIndex<T>(i);

                yield return operationResult.IsSuccessStatusCode ? operationResult.Resource : default;
            }
        }

        internal static ItemRequestOptions GetItemNoneMatchRequestOptions(this IContainerDocument document)
            => document is null ? throw new ArgumentNullException(nameof(document)) : new ItemRequestOptions { IfNoneMatchEtag = document.ETag };

        internal static QueryRequestOptions GetQueryNoneMatchRequestOptions(this IContainerDocument document)
            => document is null ? throw new ArgumentNullException(nameof(document)) : new QueryRequestOptions { IfNoneMatchEtag = document.ETag };

        internal static async IAsyncEnumerable<T> ReadAllAsync<T>(this FeedIterator<T> feedIterator, Func<T, Task<T>> processor = null)
        {
            if (feedIterator is null)
                throw new ArgumentNullException(nameof(feedIterator));

            // we use a queue to keep the order of the returned
            // documents instead of a list with the usual WaitAny
            // approach that would result in a random result stream

            var processorQueue = new Queue<Task<T>>();
            
            while (feedIterator.HasMoreResults)
            {
                var queryResponse = await feedIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                {
                    if (processor is null)
                    {
                        yield return queryResult;
                    }
                    else
                    {
                        processorQueue.Enqueue(Task.Run(() => processor(queryResult)));
                    }
                }
            }

            while (processorQueue.TryDequeue(out var task))
                yield return await task.ConfigureAwait(false);
        }
    }
}
