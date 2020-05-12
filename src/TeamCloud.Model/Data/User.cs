/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class User : IIdentifiable, IContainerDocument, IEquatable<User>, ITags, IProperties
    {
        [JsonIgnore]
        public IList<string> UniqueKeys => new List<string> { };

        [JsonProperty(PropertyName = "_etag")]
        public string ETagInternal { internal get; set; }   // for CosmosDB so we can get the ETag but doesn't get serialized to CosmosDB

        [JsonIgnore]
        public string ETag => ETagInternal;

        public string PartitionKey => Constants.CosmosDb.TenantName;

        public Guid Id { get; set; }

        string IContainerDocument.Id => this.Id.ToString();

        public UserType UserType { get; set; }

        public TeamCloudUserRole Role { get; set; }

        public IList<ProjectMembership> ProjectMemberships { get; set; } = new List<ProjectMembership>();

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public bool Equals(User other) => Id.Equals(other?.Id);

        public override bool Equals(object obj)
            => base.Equals(obj) || this.Equals(obj as User);

        public override int GetHashCode()
            => this.Id.GetHashCode();
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
            => (obj ?? throw new ArgumentNullException(nameof(obj))).Id.GetHashCode();
    }
}
