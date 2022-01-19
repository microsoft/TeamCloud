/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public class RepositoryReference
{
    [JsonProperty(Required = Required.Always)]
    public string Url { get; set; }

    public string Token { get; set; }

    public string Version { get; set; }

    public string BaselUrl { get; set; }

    public string MountUrl { get; set; }

    public string Ref { get; set; }

    [JsonProperty(Required = Required.Always)]
    public RepositoryProvider Provider { get; set; }

    [JsonProperty(Required = Required.Always)]
    public RepositoryReferenceType Type { get; set; }

    public string Organization { get; set; }

    public string Repository { get; set; }

    public string Project { get; set; }
}
