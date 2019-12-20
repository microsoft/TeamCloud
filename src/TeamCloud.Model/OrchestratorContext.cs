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

        public IUser User { get; set; }

        public OrchestratorContext (TeamCloudInstance teamCloud, Project project = null, IUser user = null)
        {
            TeamCloud = teamCloud;
            Project = project;
            User = user;
        }
    }
}
