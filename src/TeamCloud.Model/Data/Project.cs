/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class Project : ContainerDocument, IOrganizationContext, ISlug, IEquatable<Project>, IResourceReference
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        [UniqueKey]
        [JsonProperty(Required = Required.Always)]
        public string Slug => (this as ISlug).GetSlug();

        [UniqueKey]
        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Template { get; set; }

        public string TemplateInput { get; set; }

        [DatabaseIgnore]
        public IList<User> Users { get; set; } = new List<User>();

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public string ResourceId { get; set; }

        public ResourceState ResourceState { get; set; } = ResourceState.Pending;

        public string VaultId { get; set; }

        public string StorageId { get; set; }

        public bool Equals(Project other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Project);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
