/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{

    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class TeamCloudInstance : ITeamCloudInstance, IValidatable
    {
        public string Version { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }
}
