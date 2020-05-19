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

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectListByIdActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectListByIdActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectListByIdActivity))]
        public async Task<IEnumerable<Project>> RunActivity(
            [ActivityTrigger] IList<Guid> projectIds)
        {
            var projects = projectsRepository
                .ListAsync(projectIds);

            return await projects
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
