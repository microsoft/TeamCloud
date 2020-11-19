/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class RepositoryReference
    {
        // public string Id => $"{Organization}.{Project}.{Repository}".Replace("..", ".", StringComparison.Ordinal);

        [JsonProperty(Required = Required.Always)]
        public string Url { get; set; }

        public string Token { get; set; }

        public string Version { get; set; }

        public string BaselUrl { get; set; }

        public string MountUrl { get; set; }

        // [JsonProperty(Required = Required.Always)]
        public string Ref { get; set; }

        [JsonProperty(Required = Required.Always)]
        public RepositoryProvider Provider { get; set; }

        [JsonProperty(Required = Required.Always)]
        public RepositoryReferenceType Type { get; set; }

        // [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        // [JsonProperty(Required = Required.Always)]
        public string Repository { get; set; }

        public string Project { get; set; }
    }
}
