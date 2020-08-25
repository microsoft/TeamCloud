/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class TeamCloudApplication
    {
        public string Url { get; set; }

        public string Version { get; set; }

        public TeamCloudApplicaitonType Type { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }
    }
}
