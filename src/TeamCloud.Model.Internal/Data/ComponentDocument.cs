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
        public string OfferId { get; set; }

        [PartitionKey]
        public string ProjectId { get; set; }

        public string ProviderId { get; set; }

        public string RequesterId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public object Input { get; set; }

        public object Value { get; set; }
    }
}
