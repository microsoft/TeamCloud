/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class Deployment : ContainerDocument, IEquatable<Deployment>, IValidatable
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string ComponentId { get; set; }

        public DateTimeOffset Created { get; }
            = DateTimeOffset.UtcNow;

        public DateTimeOffset? Finished { get; set; }

        public DeploymentState State { get; set; }
            = DeploymentState.Pending;

        public string Log { get; set; }

        public bool Equals(Deployment other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Deployment);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
