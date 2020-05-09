using Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    internal static class CosmosDbExtensions
    {
        internal static T ToContainerDocument<T>(this ItemResponse<T> response)
            where T: class, IContainerDocument, new()
        {
            var containerDocument = response.Value;

            containerDocument.ETag = response.ETag;

            return containerDocument;
        }
    }
}
