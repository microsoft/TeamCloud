/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Project : IContainerDocument
    {
        public string PartitionKey => TeamCloudId;

        public Guid Id { get; set; }

        public string Name { get; set; }

        public AzureIdentity Identity { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public string TeamCloudId { get; set; }

        public string TeamCloudApplicationInsightsKey { get; set; }

        public List<User> Users { get; set; } = new List<User>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, Dictionary<string, string>> ProviderVariables { get; set; } = new Dictionary<string, Dictionary<string, string>>();
    }
}
