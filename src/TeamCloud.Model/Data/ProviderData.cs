/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{


    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProviderData : IProviderData, IValidatable
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public object Value { get; set; }

        public string Location { get; set; }

        public bool IsSecret { get; set; }

        public bool IsShared { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ProviderDataScope Scope { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ProviderDataType DataType { get; set; }

        public string StringValue => Value.ToString();
    }
}
