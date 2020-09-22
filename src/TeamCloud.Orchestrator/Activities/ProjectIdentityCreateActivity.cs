/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectIdentityCreateActivity
    {
        private readonly IAzureDirectoryService azureDirectoryService;

        public ProjectIdentityCreateActivity(IAzureDirectoryService azureDirectoryService)
        {
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
        }

        [FunctionName(nameof(ProjectIdentityCreateActivity)), RetryOptions(3)]
        public async Task<ProjectIdentity> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            ILogger log)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            try
            {
                var project = activityContext.GetInput<ProjectDocument>();

                if (string.IsNullOrEmpty(project.Identity?.Id))
                {
                    var servicePrincipal = await azureDirectoryService
                        .CreateServicePrincipalAsync(project.Id.ToString())
                        .ConfigureAwait(false);

                    try
                    {
                        var projectIdentity = new ProjectIdentity
                        {
                            Id = servicePrincipal.ObjectId.ToString(),
                            ApplicationId = servicePrincipal.ApplicationId,
                            TenantId = servicePrincipal.TenantId,
                            Secret = servicePrincipal.Password
                        };

                        return projectIdentity;
                    }
                    catch
                    {
                        await azureDirectoryService
                            .DeleteServicePrincipalAsync(project.Id.ToString())
                            .ConfigureAwait(false);
                    }
                }

                return project.Identity;
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"{nameof(ProjectIdentityCreateActivity)} failed with error: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}
