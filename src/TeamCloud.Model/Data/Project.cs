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
    public sealed class Project : ContainerDocument, IOrganizationContext, ISlug, IEquatable<Project>, IResourceReference
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        private string slug;

        [UniqueKey]
        [JsonProperty(Required = Required.Always)]
        public string Slug
        {
            get => slug ?? ISlug.CreateSlug(this);
            set => slug = value;
        }

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

        [Obsolete("Use SharedVaultId instead - the property only exists for backward compatibility")]
        public string VaultId => SharedVaultId;

        public string SharedVaultId { get; set; }

        public string SecretsVaultId { get; set; }

        public string StorageId { get; set; }

        public bool Equals(Project other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Project);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
