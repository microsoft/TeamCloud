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

namespace TeamCloud.Model.Data
{

    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class User : ContainerDocument, IOrganizationChild, IEquatable<User>, IProperties
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        [JsonProperty(Required = Required.Always)]
        public UserType UserType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public OrganizationUserRole Role { get; set; }

        [JsonProperty(Required = Required.Always)]
        public IList<ProjectMembership> ProjectMemberships { get; set; } = new List<ProjectMembership>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();


        public bool Equals(User other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as User);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public class UserComparer : IEqualityComparer<User>
    {
        public bool Equals(User x, User y)
        {
            if (ReferenceEquals(x, y))
                return true;
            else if (x == null || y == null)
                return false;
            else if (x.Id == y.Id)
                return true;
            else
                return false;
        }

        public int GetHashCode(User obj)
            => (obj ?? throw new ArgumentNullException(nameof(obj))).Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
