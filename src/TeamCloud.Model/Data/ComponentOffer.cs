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
    public sealed class ComponentOffer : ContainerDocument, IOrganizationChild, IValidatable
    {
        [PartitionKey]
        public string Organization { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ProviderId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string InputJsonSchema { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ComponentScope Scope { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ComponentType Type { get; set; }


        public bool Equals(ComponentOffer other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ComponentOffer);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
