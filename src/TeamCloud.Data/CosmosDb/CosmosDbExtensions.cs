using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

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

                yield return operationResult.IsSuccessStatusCode ? operationResult.Resource : default(T);
            }
        }
    }
}
