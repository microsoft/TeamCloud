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
    public sealed class Organization : ContainerDocument, IEquatable<Organization>, ITags
    {
        [PartitionKey]
        public string Tenant { get; set; }

        [UniqueKey]
        public string Slug { get; set; }

        public string DisplayName { get; set; }

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public bool Equals(Organization other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Organization);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
