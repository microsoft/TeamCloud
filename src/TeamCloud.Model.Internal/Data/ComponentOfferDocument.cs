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
    public sealed class ComponentOfferDocument
        : ContainerDocument, IComponentOffer, IPopulate<ComponentOffer>
    {
        [PartitionKey]
        public string Tenant { get; set; }

        public string ProviderId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string InputJsonSchema { get; set; }

        public ComponentScope Scope { get; set; }

        public ComponentType Type { get; set; }
    }
}
