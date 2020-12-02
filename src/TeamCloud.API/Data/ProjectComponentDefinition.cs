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
    public sealed class ProjectComponentDefinition : IValidatable
    {
        [JsonProperty(Required = Required.Always)]
        public string TemplateId { get; set; }

        public string InputJson { get; set; }

        public string DeploymentScopeId { get; set; }
    }
}
