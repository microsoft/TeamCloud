/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;
using TeamCloud.Validation;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectDefinition : ISlug, IValidatable
    {
        public string Slug => ISlug.CreateSlug(this);

        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Template { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string TemplateInput { get; set; }

        public List<UserDefinition> Users { get; set; } = new List<UserDefinition>();

        // public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        // public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
