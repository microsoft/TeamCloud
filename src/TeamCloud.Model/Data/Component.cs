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
    [SoftDelete(60 * 60 * 24)] // 24 hours
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class Component : ContainerDocument, ISoftDelete, IProjectContext, IEquatable<Component>, IValidatable, ISlug, IResourceReference
    {
        /// <summary>
        /// Gets or sets a browsable link pointing to the component resource.
        /// </summary>
        [JsonProperty("href")]
        public string HRef { get; set; }

        /// <summary>
        /// Gets or sets the organization this component belongs to.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        /// <summary>
        /// Gets or sets the template identifier this component is based on.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the project identifier this component belongs to.
        /// </summary>
        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the identity of the user that created the component.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Creator { get; set; }

        /// <summary>
        /// Gets or sets the component's display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the component's description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the input json.
        /// </summary>
        public string InputJson { get; set; }

        /// <summary>
        /// Gets or sets the value json.
        /// </summary>
        public string ValueJson { get; set; }

        /// <summary>
        /// Get or set the type of the component
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public ComponentType Type { get; set; }

        /// <summary>
        /// Get or set the Azure resource ID (subscription or resource group) this component is linked to
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the state of the resource.
        /// </summary>
        public ResourceState ResourceState { get; set; } = ResourceState.Pending;

        /// <summary>
        /// Gets or sets the deployment scope identifier for this component
        /// </summary>
        public string DeploymentScopeId { get; set; }

        /// <summary>
        /// Gets or sets the managed identity used by this component
        /// </summary>
        public string IdentityId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the component was deleted.
        /// </summary>
        public DateTime? Deleted { get; set; }

        /// <summary>
        /// Gets or sets the time to live once the component is soft deleted.
        /// </summary>
        public int? TTL { get; set; }

        /// <summary>
        /// Gets the slug of the current component base on its display name.
        /// </summary>
        [UniqueKey]
        [JsonProperty(Required = Required.Always)]
        public string Slug => (this as ISlug).GetSlug();

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Component other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Component);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
