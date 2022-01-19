/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;

namespace TeamCloud.API.Data;

public sealed class ProjectIdentityDefinition
{
    [JsonProperty(Required = Required.Always)]
    public string DisplayName { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string DeploymentScopeId { get; set; }
}
