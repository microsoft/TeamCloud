/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;
using TeamCloud.Validation;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class DeploymentScopeDefinition : ISlug, IValidatable
    {
        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DeploymentScopeType Type { get; set; }

        public string Slug => ISlug.CreateSlug(this);

        public string InputData { get; set; }

        public bool IsDefault { get; set; }
    }
}
