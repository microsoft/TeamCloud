/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model.Commands
{
    public class ProviderApi
    {
        public string SystemData { get; private set; }

        public string Project { get; private set; }

        public string ProjectData { get; private set; }

        public string ProjectUsers { get; private set; }

        public string ProjectIdentity { get; private set; }

        public string ProjectUser(string userId)
            => string.IsNullOrEmpty(ProjectUsers) ? null : new Uri($"{ProjectUsers}/{userId}").ToString();

        public ProviderApi(Uri api, string providerId = null, string projectId = null)
        {
            if (api is null) return;

            SystemData = string.IsNullOrEmpty(providerId) ? null : new Uri(api, $"api/providers/{providerId}").ToString();

            if (!string.IsNullOrEmpty(projectId))
            {
                var projectUri = new Uri(api, $"api/projects/{projectId}");

                Project = projectUri.ToString();
                ProjectUsers = new Uri(projectUri, "users").ToString();
                ProjectIdentity = new Uri(projectUri, "identity").ToString();

                if (!string.IsNullOrEmpty(providerId))
                {
                    ProjectData = new Uri(projectUri, $"providers/{providerId}/data").ToString();
                }
            }
        }
    }
}
