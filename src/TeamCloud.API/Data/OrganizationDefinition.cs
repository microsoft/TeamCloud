/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;
using TeamCloud.Validation;

namespace TeamCloud.API.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public class OrganizationDefinition : ISlug, IValidatable
{
    public string Slug => ISlug.CreateSlug(this);

    [JsonProperty(Required = Required.Always)]
    public string DisplayName { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string SubscriptionId { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Location { get; set; }

}
