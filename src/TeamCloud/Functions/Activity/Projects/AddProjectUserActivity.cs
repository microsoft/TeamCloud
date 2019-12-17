/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class AddProjectUserActivity
    {
        [FunctionName(nameof(AddProjectUserActivity))]
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
