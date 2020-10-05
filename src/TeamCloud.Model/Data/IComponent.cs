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
    public interface IComponent : IIdentifiable
    {
        string OfferId { get; set; }

        string ProjectId { get; set; }

        string ProviderId { get; set; }

        string RequesterId { get; set; }

        string DisplayName { get; set; }

        string Description { get; set; }

        object Input { get; set; }

        object Value { get; set; }
    }
}
