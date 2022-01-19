/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Activities.Projects;

public sealed class ProjectGetActivity
{
    private readonly IProjectRepository projectRepository;

    public ProjectGetActivity(IProjectRepository projectRepository)
    {
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
    }

    [FunctionName(nameof(ProjectGetActivity))]
    public async Task<Project> Run(
        [ActivityTrigger] IDurableActivityContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var input = context.GetInput<Input>();

        return await projectRepository
            .GetAsync(input.Organization, input.Id)
            .ConfigureAwait(false);
    }

    internal struct Input
    {
        public string Id { get; set; }

        public string Organization { get; set; }
    }
}
