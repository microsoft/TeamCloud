/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class TeamCloudInstanceDocument : ContainerDocument, ITeamCloudInstance, IPopulate<Model.Data.TeamCloudInstance>
    {
        [PartitionKey]
        public override string Id { get; set; }

        public string Version { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public IList<TeamCloudApplication> Applications { get; set; } = new List<TeamCloudApplication>();
    }
}
