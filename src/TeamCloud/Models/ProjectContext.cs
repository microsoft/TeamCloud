/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ProjectContext
    {
        public string UserId { get; set; }

        public string ProjectId { get; set; }

        public string ProjectName { get; set; }

        public AzureResourceGroup ProjectResourceGroup { get; set; }

        public List<ProjectUser> ProjectUsers { get; set; }

        public Dictionary<string,string> ProjectTags { get; set; }

        public Dictionary<string, Dictionary<string, string>> ProjectProviderVariables { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        public string TeamCloudId { get; set; }

        public string TeamCloudApplicationInsightsKey { get; set; }

        public List<TeamCloudUser> TeamCloudAdminUsers { get; set; }

        public Dictionary<string,string> TeamCloudTags { get; set; }

        public Dictionary<string, string> TeamCloudVariables { get; set; }

        public Dictionary<string, Dictionary<string, string>> TeamCloudProviderVariables { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        public ProjectContext(TeamCloud teamCloud, Project project, string userId)
        {
            UserId = userId;
            ProjectId = project.Id;
            ProjectName = project.Name;
            ProjectResourceGroup = project.ResourceGroup;
            ProjectUsers = project.Users;
            ProjectTags = project.Tags;

            TeamCloudId = teamCloud.Id;
            TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey;
            TeamCloudAdminUsers = teamCloud.Users.Where(u => u.Role == TeamCloudUserRole.Admin).ToList();
            TeamCloudTags = teamCloud.Configuration.Tags;
            TeamCloudVariables = teamCloud.Configuration.Variables;
            TeamCloudProviderVariables = teamCloud.Configuration.Providers.Select(p => (p.Id, p.Variables)).ToDictionary(t => t.Id, t => t.Variables);
        }

        public ProjectContext(OrchestratorContext orchestratorContext)
            : this (orchestratorContext.TeamCloud, orchestratorContext.Project, orchestratorContext.User.Id)
        { }
    }
}
