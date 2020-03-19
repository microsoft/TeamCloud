/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AzureResourceGroup : IIdentifiable, IEquatable<AzureResourceGroup>
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SubscriptionId { get; set; } = Guid.Empty;

        public string ResourceGroupId { get; set; }

        public string ResourceGroupName { get; set; }

        public string Region { get; set; }

        public bool Equals(AzureResourceGroup other)
            => Id.Equals(other?.Id);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as AzureResourceGroup);

        public override int GetHashCode()
            => Id.GetHashCode();
    }
}
