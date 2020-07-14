/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Azure.Directory;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Model.Internal.Data;
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
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            try
            {
                var project = functionContext.GetInput<Project>();

                var keyVault = await azureResourceService
                    .GetResourceAsync<AzureKeyVaultResource>(project.KeyVault.VaultId, throwIfNotExists: true)
                    .ConfigureAwait(false);

                var projectIdentityJson = await keyVault
                    .GetSecretAsync(nameof(ProjectIdentity))
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(projectIdentityJson))
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

                        projectIdentityJson = JsonConvert.SerializeObject(projectIdentity);

                        await keyVault
                            .SetSecretAsync(nameof(ProjectIdentity), projectIdentityJson)
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        await azureDirectoryService
                            .DeleteServicePrincipalAsync(project.Id.ToString())
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"{nameof(ProjectIdentityCreateActivity)} failed with error: {ex.Message}");
                throw;
            }
        }
    }
}
