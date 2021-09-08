/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
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
    }
}
