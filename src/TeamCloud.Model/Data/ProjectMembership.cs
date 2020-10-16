/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectMembership : IProperties
    {
        [JsonProperty(Required = Required.Always)]
        public string ProjectId { get; set; }

        public ProjectUserRole Role { get; set; }

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
