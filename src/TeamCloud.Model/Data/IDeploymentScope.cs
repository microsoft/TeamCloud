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
    public interface IDeploymentScope : IIdentifiable, IDisplayName
    {
        string ManagementGroupId { get; set; }

        bool IsDefault { get; set; }
    }
}
