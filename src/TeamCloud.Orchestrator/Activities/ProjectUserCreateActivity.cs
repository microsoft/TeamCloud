/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class ProjectUserCreateActivity
    {
        [FunctionName(nameof(ProjectUserCreateActivity))]
        public static Project RunActivity(
            [ActivityTrigger] (Project project, ProjectUser newUser) input,
            [CosmosDB(nameof(TeamCloud), "Projects", Id = "{input.project.id}", PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] Project project)
        {
            if (project.Users == null)
                project.Users = new List<ProjectUser>();

            project.Users.Add(input.newUser);

            return project;
        }
    }
}
