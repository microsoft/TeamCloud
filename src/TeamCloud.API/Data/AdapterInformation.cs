/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class AdapterInformation
    {
        public DeploymentScopeType Type { get; set; }

        public string DisplayName { get; set; }

        public string InputDataSchema { get; set; }

        public string InputDataForm { get; set; }
    }
}
