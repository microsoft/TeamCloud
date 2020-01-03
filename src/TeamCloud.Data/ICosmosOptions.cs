/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Data
{
    public interface ICosmosOptions
    {
        string AzureCosmosDBName { get; set; }

        string AzureCosmosDBConnection { get; set; }
    }
}
