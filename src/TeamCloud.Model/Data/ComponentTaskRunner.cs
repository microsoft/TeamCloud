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
    public class ComponentTaskRunner
    {
        public string Id { get; set; }

        public Dictionary<string, string> With { get; set; } = new Dictionary<string, string>();
    }
}
