/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class OrganizationDocument : ContainerDocument, IOrganization, IEquatable<OrganizationDocument>, IValidatable
    {
        [PartitionKey]
        public string Tenant { get; set; }

        public string DisplayName { get; set; }

        public string InputJsonSchema { get; set; }

        public bool Equals(OrganizationDocument other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as OrganizationDocument);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
