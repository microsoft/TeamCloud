/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;

namespace TeamCloud.Model.Common
{
    public interface ISoftDelete
    {
        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        int? TTL { get; set; }

        [JsonProperty(PropertyName = "deleted", NullValueHandling = NullValueHandling.Ignore)]
        DateTime? Deleted { get; set; }
    }
}
