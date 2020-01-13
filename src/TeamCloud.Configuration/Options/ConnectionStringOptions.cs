/**
*  Copyright (c) Microsoft Corporation.
*  Licensed under the MIT License.
*/

namespace TeamCloud.Configuration.Options
{
    [Options("ConnectionStrings")]
    public class ConnectionStringOptions
    {
        public string CosmosDbConnectionString { get; set; }
    }
}
