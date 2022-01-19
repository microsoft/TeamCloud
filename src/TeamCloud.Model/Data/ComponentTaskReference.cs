/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public sealed class ComponentTaskReference
{
    public string ComponentId { get; set; }

    public string ComponentTaskTemplateId { get; set; }

    public string InputJson { get; set; }
}
