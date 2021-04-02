/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Broadcast
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class BroadcastMessage
    {
        public string Action { get; set; }

        [JsonProperty("ts")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public IEnumerable<Item> Items { get; set; } = new List<Item>();

        [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
        public sealed class Item
        {
            public string Id { get; set; }
            public string Organization { get; set; }

            public string Project { get; set; }

            public string Component { get; set; }

            public string Type { get; set; }

            [JsonProperty("etag")]
            public string ETag { get; set; }

            [JsonProperty("ts")]
            public DateTime? Timestamp { get; set; }
        }
    }
}
