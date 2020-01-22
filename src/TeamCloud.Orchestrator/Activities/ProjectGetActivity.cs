/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectGetActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectGetActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectGetActivity))]
        public IAsyncEnumerable<Project> RunActivity(
            [ActivityTrigger] TeamCloudInstance teamCloud)
        {
            if (teamCloud is null)
                throw new ArgumentNullException(nameof(teamCloud));

            return projectsRepository.ListAsync();
        }
    }
}
