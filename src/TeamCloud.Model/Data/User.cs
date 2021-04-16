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

namespace TeamCloud.Model.Data
{

    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class User : ContainerDocument, IOrganizationContext, IEquatable<User>, IProperties, INotificationRecipient
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

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

        // [JsonProperty(Required = Required.Always)]
        public IList<ProjectMembership> ProjectMemberships { get; set; } = new List<ProjectMembership>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        string INotificationRecipient.Address { get => MailAddress ?? Id; }

        public bool Equals(User other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as User);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
