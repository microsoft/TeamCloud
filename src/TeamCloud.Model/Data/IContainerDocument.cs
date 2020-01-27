/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public interface IContainerDocument
    {
        const string PartitionKeyPath = "/partitionKey";

        string PartitionKey { get; }

        List<string> UniqueKeys { get; }
    }
}
