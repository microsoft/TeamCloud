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
    public sealed class Component : ContainerDocument, IOrganizationChild, IEquatable<Component>, IValidatable
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
        /// Gets or sets the provider this component should use for communication.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        [JsonProperty(Required = Required.Always)]
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets the identity the component was requested by.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string RequestedBy { get; set; }

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
        [JsonProperty(Required = Required.Always)]
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
