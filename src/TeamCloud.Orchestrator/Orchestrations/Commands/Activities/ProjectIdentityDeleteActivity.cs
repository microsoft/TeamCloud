using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Directory;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    public class ProjectIdentityDeleteActivity
    {
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly IAzureResourceService azureResourceService;

        public ProjectIdentityDeleteActivity(IAzureDirectoryService azureDirectoryService, IAzureResourceService azureResourceService)
        {
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ProjectIdentityDeleteActivity)), RetryOptions(3)]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var project = functionContext.GetInput<Project>();

            var keyVault = await azureResourceService
                .GetResourceAsync<AzureKeyVaultResource>(project.KeyVault.VaultId, throwIfNotExists: true)
                .ConfigureAwait(false);

            var projectIdentityJson = await keyVault
                .GetSecretAsync(nameof(ProjectIdentity))
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(projectIdentityJson))
            {
                await azureDirectoryService
                    .DeleteServicePrincipalAsync(project.Id.ToString())
                    .ConfigureAwait(false);
            }

            await keyVault
                .SetSecretAsync(nameof(ProjectIdentity), null)
                .ConfigureAwait(false);
        }
    }
}
