/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class DeploymentScopeDefinition
    {
        public string DisplayName { get; set; }

        public string ManagementGroupId { get; set; }

        public bool IsDefault { get; set; }
    }
}
