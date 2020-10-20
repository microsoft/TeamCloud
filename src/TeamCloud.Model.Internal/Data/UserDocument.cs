/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class UserDocument : ContainerDocument, IUser, IEquatable<UserDocument>, IPopulate<User>
    {
        [PartitionKey]
        public string Tenant { get; set; }

        public UserType UserType { get; set; }

        public TeamCloudUserRole Role { get; set; }

        public IList<ProjectMembership> ProjectMemberships { get; set; } = new List<ProjectMembership>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();


        public bool Equals(UserDocument other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as UserDocument);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public class UserComparer : IEqualityComparer<UserDocument>
    {
        public bool Equals(UserDocument x, UserDocument y)
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

        public int GetHashCode(UserDocument obj)
            => (obj ?? throw new ArgumentNullException(nameof(obj))).Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
