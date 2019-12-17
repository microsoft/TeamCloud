/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class TeamCloudConfiguraiton
    {
        public string Version { get; set; }

        public TeamCloudAzureConfiguration Azure { get; set; }

        public List<TeamCloudProviderConfiguration> Providers { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public Dictionary<string, string> Variables { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class TeamCloudAzureConfiguration
    {
        public string Region { get; set; }

        public string SubscriptionId { get; set; }

        public AzureIdentity ServicePricipal { get; set; }

        public List<string> SubscriptionPoolIds { get; set; }

        public int ProjectsPerSubscription { get; set; }

        public string ResourceGroupNamePrefix { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class TeamCloudProviderConfiguration
    {
        public string Id { get; set; }

        public Uri Location { get; set; }

        public string AuthKey { get; set; }

        public bool Optional { get; set; }

        public TeamCloudProviderConfigurationDependencies Dependencies { get; set; }

        public List<string> Events { get; set; }

        public Dictionary<string, string> Variables { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class TeamCloudProviderConfigurationDependencies
    {
        public List<string> Create { get; set; }

        public List<string> Init { get; set; }
    }
}
