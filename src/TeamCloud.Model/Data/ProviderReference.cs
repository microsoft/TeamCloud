/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ProviderReference
    {
        public string Id { get; set; }

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public IList<string> DependsOn { get; set; } = new List<string>();

        public IDictionary<string, IDictionary<string, string>> Metadata { get; set; } = new Dictionary<string, IDictionary<string, string>>();
    }
}
