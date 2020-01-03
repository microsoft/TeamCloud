/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class ProjectUserDeleteActivity
    {
        [FunctionName(nameof(ProjectUserDeleteActivity))]
        public static Project RunActivity(
            [ActivityTrigger] (Project project, User deleteUser) input,
            [CosmosDB(Constants.CosmosDb.DatabaseName, nameof(Project), Id = "{input.project.id}", PartitionKey = Constants.CosmosDb.TeamCloudInstanceId, ConnectionStringSetting = "AzureCosmosDBConnection")] Project project)
        {
            var user = project.Users?.FirstOrDefault(u => u.Id == input.deleteUser.Id);

            if (user != null)
            {
                project.Users.Remove(user);
            }

            return project;
        }
    }
}
