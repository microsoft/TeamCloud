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

        string RequestedBy { get; set; }

        string DisplayName { get; set; }

        string Description { get; set; }

        string InputJson { get; set; }

        string ValueJson { get; set; }
    }
}
