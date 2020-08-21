/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectLinkDefinition : IValidatable
    {
        public string Id { get; set; }
            = Guid.NewGuid().ToString();

        [JsonProperty("href")]
        public string HRef { get; set; }

        public string Title { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProjectLinkType Type { get; set; }
            = ProjectLinkType.Undefined;
    }
}
