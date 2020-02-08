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
    public sealed class TeamCloudConfiguration : IValidatable
    {
        public List<ProjectType> ProjectTypes { get; set; } = new List<ProjectType>();

        public List<Provider> Providers { get; set; } = new List<Provider>();

        public List<User> Users { get; set; } = new List<User>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
