/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class ProviderReference
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }

        public IList<string> DependsOn { get; set; } = new List<string>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, IDictionary<string, string>> Metadata { get; set; } = new Dictionary<string, IDictionary<string, string>>();
    }
}
