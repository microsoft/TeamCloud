/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Context
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ProjectContext
    {
        public User User { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public AzureResourceGroup ProjectResourceGroup { get; set; }

        public List<User> ProjectUsers { get; set; }

        public Dictionary<string, string> ProjectTags { get; set; }

        public Dictionary<string, Dictionary<string, string>> ProjectProviderVariables { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        public string TeamCloudId { get; set; }

        public string TeamCloudApplicationInsightsKey { get; set; }

        public List<User> TeamCloudAdminUsers { get; set; }

        public Dictionary<string, string> TeamCloudTags { get; set; }

        public Dictionary<string, string> TeamCloudVariables { get; set; }

        public Dictionary<string, Dictionary<string, string>> TeamCloudProviderVariables { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        public ProjectContext(TeamCloudInstance teamCloud, Project project, User user)
        {
            User = user;
            ProjectId = project.Id;
            ProjectName = project.Name;
            ProjectResourceGroup = project.ResourceGroup;
            ProjectUsers = project.Users;
            ProjectTags = project.Tags;

            TeamCloudId = teamCloud.Id;
            TeamCloudApplicationInsightsKey = teamCloud.ApplicationInsightsKey;
            TeamCloudAdminUsers = teamCloud.Users.Where(u => u.Role == UserRoles.TeamCloud.Admin).ToList();
            TeamCloudTags = teamCloud.Configuration.Tags;
            TeamCloudVariables = teamCloud.Configuration.Variables;
            TeamCloudProviderVariables = teamCloud.Configuration.Providers.Select(p => (p.Id, p.Variables)).ToDictionary(t => t.Id, t => t.Variables);
        }

        public ProjectContext(OrchestratorContext orchestratorContext, User user)
            : this(orchestratorContext.TeamCloud, orchestratorContext.Project, user)
        { }
    }
}
