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
        public string Organization { get; set; }

        public string ParentId { get; set; }

        public string ProviderId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public RepositoryReference Repository { get; set; }

        public string InputJsonSchema { get; set; }

        public ComponentScope Scope { get; set; }

        public ComponentType Type { get; set; }


        public bool Equals(ComponentTemplate other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ComponentTemplate);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
