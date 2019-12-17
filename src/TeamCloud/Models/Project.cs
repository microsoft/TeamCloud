/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Project
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public AzureIdentity Identity { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public string TeamCloudId { get; set; }

        public string TeamCloudApplicationInsightsKey { get; set; }

        public List<ProjectUser> Users { get; set; }

        public Dictionary<string,string> Tags { get; set; }

        public Dictionary<string, Dictionary<string, string>> ProviderVariables { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ProjectDefinition
    {
        public string Name { get; set; }

        public Dictionary<string,string> Tags { get; set; }

        public List<ProjectUserDefinition> Users { get; set; }
    }
}
