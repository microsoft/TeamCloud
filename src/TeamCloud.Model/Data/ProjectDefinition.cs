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
    public class ProjectDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public List<UserDefinition> Users { get; set; }
    }
}
