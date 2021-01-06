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
        private string typeName;

        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string ComponentId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ProjectId { get; set; }

        public string StorageId { get; set; }

        public ComponentDeploymentType Type { get; set; } = ComponentDeploymentType.Create;

        public string TypeName
        {
            get => Type == ComponentDeploymentType.Custom ? typeName : default;
            set => typeName = value;
        }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Started { get; set; }

        public DateTime? Finished { get; set; }

        [DatabaseIgnore]
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
