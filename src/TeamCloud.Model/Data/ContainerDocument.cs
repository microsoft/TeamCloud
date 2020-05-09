using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public interface IContainerDocument
    {
        const string PartitionKeyPath = "/partitionKey";

        string Id { get; }

        string PartitionKey { get; }

        [JsonIgnore]
        IList<string> UniqueKeys { get; }

        [JsonIgnore]
        string ETag { get; set; }
    }
}
