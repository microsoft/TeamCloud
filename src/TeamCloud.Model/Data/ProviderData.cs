/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{


    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProviderData : IProviderData
    {
        // public string ProviderId { get; set; }

        // public string ProjectId { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public string Location { get; set; }

        public bool IsSecret { get; set; }

        public bool IsShared { get; set; }

        public ProviderDataScope Scope { get; set; }

        public ProviderDataType DataType { get; set; }

        public string StringValue => Value.ToString();
    }
}
