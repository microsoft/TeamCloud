/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;
using TeamCloud.Validation;

namespace TeamCloud.API.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public class RepositoryDefinition : IValidatable
{
    [JsonProperty(Required = Required.Always)]
    public string Url { get; set; }

    public string Token { get; set; }

    public string Version { get; set; }
}
