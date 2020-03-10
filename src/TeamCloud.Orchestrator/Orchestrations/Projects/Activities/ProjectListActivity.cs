/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Projects.Activities
{
    public class ProjectListActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectListActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectListActivity))]
        public async Task<IEnumerable<Project>> RunActivity(
            [ActivityTrigger] TeamCloudInstance teamCloud)
        {
            var projects = projectsRepository
                .ListAsync();

            return await projects
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
