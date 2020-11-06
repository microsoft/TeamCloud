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
    public sealed class DeploymentScopeDocument : ContainerDocument, IDeploymentScope, IEquatable<DeploymentScopeDocument>, IPopulate<DeploymentScope>, IValidatable
    {
        [PartitionKey]
        public string Tenant { get; set; }

        public string DisplayName { get; set; }

        public string ManagementGroupId { get; set; }

        public bool IsDefault { get; set; }

        public bool Equals(DeploymentScopeDocument other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as DeploymentScopeDocument);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
