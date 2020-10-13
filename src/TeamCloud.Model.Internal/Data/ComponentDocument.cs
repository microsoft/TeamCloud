/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ComponentDocument
        : ContainerDocument, IComponent, IPopulate<Component>
    {
        [JsonProperty("href")]
        public string HRef { get; set; }

        public string OfferId { get; set; }

        [PartitionKey]
        public string ProjectId { get; set; }

        public string ProviderId { get; set; }

        public string RequestedBy { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string InputJson { get; set; }

        public string ValueJson { get; set; }

        public ComponentScope Scope { get; set; }

        public ComponentType Type { get; set; }
    }
}
