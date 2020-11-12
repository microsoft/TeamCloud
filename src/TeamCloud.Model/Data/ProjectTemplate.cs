/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectTemplate : ContainerDocument, ISlug, IOrganizationChild, IRepositoryReference, IEquatable<ProjectTemplate>, IValidatable
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        [UniqueKey]
        [JsonProperty(Required = Required.Always)]
        public string Slug => (this as ISlug).GetSlug();

        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        public List<string> Components { get; set; } = new List<string>();

        [JsonProperty(Required = Required.Always)]
        public RepositoryReference Repository { get; set; }

        public string Description { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool IsDefault { get; set; }

        public string InputJsonSchema { get; set; }


        public bool Equals(ProjectTemplate other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProjectTemplate);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
