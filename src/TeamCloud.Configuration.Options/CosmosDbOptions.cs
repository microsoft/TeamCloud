/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Configuration.Options
{
    [Options("Azure:CosmosDb")]
    public class CosmosDbOptions
    {
        public string Tenant { get; set; }

        public string DatabaseName { get; set; } = "TeamCloud";

        public string ConnectionString { get; set; }
    }
}
