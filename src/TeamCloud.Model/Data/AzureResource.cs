/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AzureResourceGroup
    {
        public string SubscriptionId { get; set; }

        public string ResourceGroupId { get; set; }

        public string ResourceGroupName { get; set; }

        public string Region { get; set; }
    }
}
