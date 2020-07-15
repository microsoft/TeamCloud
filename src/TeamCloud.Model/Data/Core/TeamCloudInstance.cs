/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public interface ITeamCloudInstance : ITags
    {
        string Version { get; set; }

        AzureResourceGroup ResourceGroup { get; set; }
    }
}
