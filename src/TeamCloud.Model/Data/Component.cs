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
    public sealed class Component : ContainerDocument, IEquatable<Component>, IValidatable
    {
        [JsonProperty("href")]
        public string HRef { get; set; }

        public string OfferId { get; set; }

        [PartitionKey]
        public string ProjectId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ProviderId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string RequestedBy { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string InputJson { get; set; }

        public string ValueJson { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ComponentScope Scope { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ComponentType Type { get; set; }


        public bool Equals(Component other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Component);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
