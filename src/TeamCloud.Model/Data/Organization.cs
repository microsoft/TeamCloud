/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{

    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class Organization : IOrganization, IEquatable<Organization>
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; } // = Guid.NewGuid().ToString();

        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Tenant { get; set; }

        // public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public bool Equals(Organization other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Organization);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
