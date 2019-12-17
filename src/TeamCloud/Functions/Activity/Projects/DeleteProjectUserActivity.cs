/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TeamCloud
{
    public static class DeleteProjectUserActivity
    {
        [FunctionName(nameof(DeleteProjectUserActivity))]
        public static Project RunActivity(
            [ActivityTrigger] (Project project, ProjectUser deleteUser) input,
            [CosmosDB(nameof(TeamCloud), "Projects", Id = "{input.project.id}", PartitionKey = nameof(TeamCloud), ConnectionStringSetting = "AzureCosmosDBConnection")] Project project)
        {
            var user = project.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
                project.Users.Remove(user);

            return project;
        }
    }
}
