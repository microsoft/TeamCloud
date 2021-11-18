/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;
using TeamCloud.Validation;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class ProjectTemplateDefinition : IDisplayName, IValidatable
    {
        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public RepositoryDefinition Repository { get; set; }
    }
}
