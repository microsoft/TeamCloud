/**
*  Copyright (c) Microsoft Corporation.
*  Licensed under the MIT License.
*/

namespace TeamCloud.Configuration.Options
{
    [Options("Azure:CosmosDb")]
    public class CosmosDbOptions
    {
        public string DatabaseName { get; set; }

        public string ConnectionString { get; set; }
    }
}
