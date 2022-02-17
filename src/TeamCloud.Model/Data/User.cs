/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Notification;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
[ContainerPath("/orgs/{Organization}/users/{Id}")]

public sealed class User : ContainerDocument, IOrganizationContext, IEquatable<User>, IProperties, INotificationRecipient
{
    [PartitionKey]
    [JsonProperty(Required = Required.Always)]
    public string Organization { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string OrganizationName { get; set; }

    [DatabaseIgnore]
    public string DisplayName { get; set; }

    [DatabaseIgnore]
    public string LoginName { get; set; }

    [DatabaseIgnore]
    public string MailAddress { get; set; }

    [JsonProperty(Required = Required.Always)]
    public UserType UserType { get; set; }

    [JsonProperty(Required = Required.Always)]
    public OrganizationUserRole Role { get; set; }

    private IList<ProjectMembership> projectMemberships;

    public IList<ProjectMembership> ProjectMemberships
    {
        get => projectMemberships ??= new List<ProjectMembership>();
        set => projectMemberships = value;
    }

    private IDictionary<DeploymentScopeType, AlternateIdentity> alternateIdentities;

    public IDictionary<DeploymentScopeType, AlternateIdentity> AlternateIdentities
    {
        get => alternateIdentities ??= new Dictionary<DeploymentScopeType, AlternateIdentity>();
        set => alternateIdentities = value;
    }

    private IDictionary<string, string> properties;

    public IDictionary<string, string> Properties
    {
        get => properties ??= new Dictionary<string, string>();
        set => properties = value;
    }

    string INotificationRecipient.Address { get => MailAddress ?? Id; }

    public bool Equals(User other)
        => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj)
        => base.Equals(obj) || Equals(obj as User);

    public override int GetHashCode()
        => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
}
