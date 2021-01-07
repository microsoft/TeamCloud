/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ComponentTaskDefinition : IValidatable
    {
        [JsonProperty(Required = Required.Always)]
        public string TaskId { get; set; }

        public string InputJson { get; set; }
    }
}
