/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class OrchestratorContext
    {
        public TeamCloudInstance TeamCloud { get; set; }

        public Project Project { get; set; }

        public OrchestratorContext(TeamCloudInstance teamCloud, Project project = null)
        {
            TeamCloud = teamCloud;
            Project = project;
        }
    }
}
