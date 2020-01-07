/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Data;

namespace TeamCloud.API.Options
{
    [Options("CosmosDb")]
    public class CosmosOptions : ICosmosOptions
    {
        public string AzureCosmosDBName { get; set; } = "TeamCloud";

        public string AzureCosmosDBConnection { get; set; }
    }
}
