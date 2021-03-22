/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Activities.Projects
{
    public sealed class ProjectSetActivity
    {
        private readonly IProjectRepository projectRepository;

        public ProjectSetActivity(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        [FunctionName(nameof(ProjectSetActivity))]
        public async Task<Project> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var project = context.GetInput<Input>().Project;

            project.ResourceState = context.GetInput<Input>().ResourceState.GetValueOrDefault(project.ResourceState);

            project = await projectRepository
                .SetAsync(project)
                .ConfigureAwait(false);

            return project;
        }

        internal struct Input
        {
            public Project Project { get; set; }

            public ResourceState? ResourceState { get; set; }
        }
    }
}
