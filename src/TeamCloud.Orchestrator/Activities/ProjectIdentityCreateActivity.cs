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
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectIdentityCreateActivity
    {
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly IAzureResourceService azureResourceService;

        public ProjectIdentityCreateActivity(IAzureDirectoryService azureDirectoryService, IAzureResourceService azureResourceService)
        {
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ProjectIdentityCreateActivity)), RetryOptions(3)]
        public async Task<ProjectIdentity> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            try
            {
                var project = functionContext.GetInput<ProjectDocument>();

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
            catch (Exception ex)
            {
                log.LogError(ex, $"{nameof(ProjectIdentityCreateActivity)} failed with error: {ex.Message}");
                throw;
            }
        }
    }
}
