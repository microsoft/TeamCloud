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
    public sealed class Organization : ContainerDocument, ISlug, IEquatable<Organization>, ITags, IResourceReference
    {
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string Tenant { get; set; }

        [UniqueKey]
        [JsonProperty(Required = Required.Always)]
        public string Slug => (this as ISlug).GetSlug();

        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string SubscriptionId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Location { get; set; }

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public string ResourceId { get; set; }

        public ResourceState ResourceState { get; set; } = ResourceState.Pending;

        public string GalleryId { get; set; }

        public string RegistryId { get; set; }

        public string StorageId { get; set; }

        public bool Equals(Organization other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Organization);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
