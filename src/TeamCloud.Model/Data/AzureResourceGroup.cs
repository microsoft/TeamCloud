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
        public string Id { get; set; }

        public string Name { get; set; }

        public Guid SubscriptionId { get; set; } = Guid.Empty;

        public string Region { get; set; }

        public bool Equals(AzureResourceGroup other)
            => Id?.Equals(other?.Id, StringComparison.OrdinalIgnoreCase) ?? false;

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as AzureResourceGroup);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? base.GetHashCode();
    }
}
