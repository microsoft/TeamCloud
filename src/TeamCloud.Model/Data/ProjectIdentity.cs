/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectIdentity : IIdentifiable, IEquatable<ProjectIdentity>
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Guid TenantId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Guid ApplicationId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Secret { get; set; }


        public bool Equals(ProjectIdentity other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProjectIdentity);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.Ordinal);
    }
}
