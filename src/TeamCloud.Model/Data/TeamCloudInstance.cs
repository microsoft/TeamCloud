/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class TeamCloudInstance : IContainerDocument
    {
        public string Id => Constants.CosmosDb.TenantName;

        public string PartitionKey => Constants.CosmosDb.DatabaseName;

        [JsonIgnore]
        public IList<string> UniqueKeys => new List<string> { };

        public AzureResourceGroup ResourceGroup { get; set; }

        public IList<User> Users { get; set; } = new List<User>();

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public IList<Provider> Providers { get; set; } = new List<Provider>();

        public TeamCloudInstance()
        { }

        public TeamCloudInstance(TeamCloudConfiguration config)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            Users = config.Users;
            Tags = config.Tags;
            Properties = config.Properties;
            Providers = config.Providers;
        }
    }
}
