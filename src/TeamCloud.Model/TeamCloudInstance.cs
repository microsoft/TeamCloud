/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class TeamCloudInstance
    {
        public string Id { get; set; }

        public string PartitionKey { get; set; } = "TeamCloud";

        public AzureResourceGroup ResourceGroup { get; set; }

        public string ApplicationInsightsKey { get; set; }

        public List<TeamCloudUser> Users { get; set; }

        public List<string> ProjectIds { get; set; }

        public TeamCloudConfiguraiton Configuration { get; set; }
    }
}
