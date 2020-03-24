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
    public class AzureResourceGroup : IEquatable<AzureResourceGroup>
    {
        public string ResourceGroupId { get; set; }

        public string ResourceGroupName { get; set; }

        public Guid SubscriptionId { get; set; } = Guid.Empty;

        public string Region { get; set; }

        public bool Equals(AzureResourceGroup other)
            => ResourceGroupId?.Equals(other?.ResourceGroupId, StringComparison.OrdinalIgnoreCase) ?? false;

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as AzureResourceGroup);

        public override int GetHashCode()
            => ResourceGroupId?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? base.GetHashCode();
    }
}
