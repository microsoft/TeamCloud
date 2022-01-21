/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
[ContainerPath("/orgs/{Organization}/scopes/{Id}")]
public sealed class DeploymentScope : ContainerDocument, IOrganizationContext, ISlug, IEquatable<DeploymentScope>
{
    [PartitionKey]
    [JsonProperty(Required = Required.Always)]
    public string Organization { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string DisplayName { get; set; }

    private string slug;

    [UniqueKey]
    [JsonProperty(Required = Required.Always)]
    public string Slug
    {
        get => slug ?? ISlug.CreateSlug(this);
        set => slug = value;
    }

    [JsonProperty(Required = Required.Always)]
    public bool IsDefault { get; set; }

    [JsonProperty(Required = Required.Always)]
    public DeploymentScopeType Type { get; set; }

    public string InputDataSchema { get; set; }

    public string InputData { get; set; }

    public string ManagementGroupId { get; set; }

    public IList<Guid> SubscriptionIds { get; set; }
        = new List<Guid>();

    [DatabaseIgnore]
    public bool Authorizable { get; set; }

    [DatabaseIgnore]
    public bool Authorized { get; set; }

    [DatabaseIgnore]
    public string AuthorizeUrl { get; set; }

    [DatabaseIgnore]
    public IList<ComponentType> ComponentTypes { get; set; }
        = new List<ComponentType>();

    public bool Equals(DeploymentScope other)
        => Id.Equals(other?.Id, StringComparison.Ordinal);

    public override bool Equals(object obj)
        => base.Equals(obj) || Equals(obj as DeploymentScope);

    public override int GetHashCode()
        => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
}
