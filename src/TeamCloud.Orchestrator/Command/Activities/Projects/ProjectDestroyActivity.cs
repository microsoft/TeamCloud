/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Activities.Projects;

public sealed class ProjectDestroyActivity
{
    private readonly IProjectRepository projectRepository;
    private readonly IAzureService azureService;

    public ProjectDestroyActivity(IProjectRepository projectRepository, IAzureService azureService)
    {
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.azureService = azureService ?? throw new ArgumentNullException(nameof(azureService));
    }

    [FunctionName(nameof(ProjectDestroyActivity))]
    [RetryOptions(3)]
    public async Task Run(
        [ActivityTrigger] IDurableActivityContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var project = context.GetInput<Input>().Project;

        if (!string.IsNullOrEmpty(project.ResourceId))
        {
            await azureService
                .DeleteResourceAsync(project.ResourceId, deleteLocks: true)
                .ConfigureAwait(false);
        }
    }

    internal struct Input
    {
        public Project Project { get; set; }
    }
}
