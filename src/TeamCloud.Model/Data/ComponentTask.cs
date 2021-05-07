/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using System;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ComponentTask : ContainerDocument, IEquatable<ComponentTask>, IValidatable, IResourceReference, IComponentContext
    {
        private string typeName;

        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string ComponentId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ProjectId { get; set; }

        public string RequestedBy { get; set; }

        public string ScheduledTaskId { get; set; }

        public ComponentTaskType Type { get; set; } = ComponentTaskType.Create;

        public string TypeName
        {
            get => Type == ComponentTaskType.Custom ? typeName : default;
            set => typeName = value;
        }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Started { get; set; }

        public DateTime? Finished { get; set; }

        public string InputJson { get; set; }

        [DatabaseIgnore]
        public string Output { get; set; }

        public string ResourceId { get; set; }

        public ResourceState ResourceState { get; set; }
            = ResourceState.Pending;

        public int? ExitCode { get; set; }

        public bool Equals(ComponentTask other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ComponentTask);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
