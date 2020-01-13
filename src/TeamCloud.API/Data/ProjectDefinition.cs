/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ProjectDefinition
    {
        public string Name { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public List<UserDefinition> Users { get; set; } = new List<UserDefinition>();
    }
}
