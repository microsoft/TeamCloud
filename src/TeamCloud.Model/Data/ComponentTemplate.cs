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
    public sealed class ComponentTemplate : ContainerDocument, IOrganizationChild, IRepositoryReference, IValidatable
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ParentId { get; set; }

        public string ProviderId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        [JsonProperty(Required = Required.Always)]
        public RepositoryReference Repository { get; set; }

        public string InputJsonSchema { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ComponentScope Scope { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ComponentType Type { get; set; }


        public bool Equals(ComponentTemplate other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ComponentTemplate);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
