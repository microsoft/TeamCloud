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
    public sealed class ComponentDeployment : ContainerDocument, IEquatable<ComponentDeployment>, IValidatable, IResourceReference
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string ComponentId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ProjectId { get; set; }

        public DateTime? Started { get; set; }

        public DateTime? Finished { get; set; }

        public string Output { get; set; }

        public string ResourceId { get; set; }

        public ResourceState ResourceState { get; set; }
            = ResourceState.Pending;

        public int? ExitCode { get; set; }

        public bool Equals(ComponentDeployment other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ComponentDeployment);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
