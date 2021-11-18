/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Serialization;
using TeamCloud.Validation;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class UserDefinition : IValidatable
    {
        [JsonProperty(Required = Required.Always)]
        public string Identifier { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Role { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
