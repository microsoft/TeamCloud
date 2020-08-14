/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Internal.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectIdentity : IIdentifiable, IEquatable<ProjectIdentity>
    {
        public string Id { get; set; }

        public Guid ApplicationId { get; set; }

        public string Secret { get; set; }


        public bool Equals(ProjectIdentity other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProjectIdentity);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.Ordinal);
    }
}
