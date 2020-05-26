/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Data.CosmosDb.Core
{
    public sealed class CosmosDbTestOptions : ICosmosDbOptions
    {
        public static readonly ICosmosDbOptions Default = new CosmosDbTestOptions()
        {
            TenantName = "TeamCloud",
            DatabaseName = "TeamCloudTest",
            ConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
        };

        public string TenantName { get; set; }

        public string DatabaseName { get; set; }

        public string ConnectionString { get; set; }
    }
}
