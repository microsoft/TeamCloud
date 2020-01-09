/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AzureResourceGroup : Identifiable, IEquatable<AzureResourceGroup>
    {
        public Guid Id { get; set; }

        public string SubscriptionId { get; set; }

        public string ResourceGroupName { get; set; }

        public string Region { get; set; }

        public bool Equals(AzureResourceGroup other) => Id.Equals(other.Id);
    }
}
