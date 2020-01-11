using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TeamCloud.Azure;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public class AzureResourceGroupCreateActivity
    {
        private readonly IAzureSessionFactory azureSessionFactory;

        public AzureResourceGroupCreateActivity(IAzureSessionFactory azureSessionFactory)
        {
            this.azureSessionFactory = azureSessionFactory ?? throw new ArgumentNullException(nameof(azureSessionFactory));
        }

        [FunctionName(nameof(AzureResourceGroupCreateActivity))]
        public async Task<string> RunActivity(
            [ActivityTrigger] AzureResourceGroup azureResourceGroup)
        {
            if (azureResourceGroup == null)
                throw new ArgumentNullException(nameof(azureResourceGroup));
            if (string.IsNullOrWhiteSpace(azureResourceGroup.ResourceGroupName))
                throw new ArgumentNullException(nameof(azureResourceGroup.ResourceGroupName));
            if (azureResourceGroup.Region == null)
                throw new ArgumentNullException(nameof(azureResourceGroup.Region));

            var azureSession = azureSessionFactory.CreateSession(Guid.Parse(azureResourceGroup.SubscriptionId));

            if (await azureSession.ResourceGroups.ContainAsync(azureResourceGroup.ResourceGroupName).ConfigureAwait(false) == false)
            {
                var newGroup = await azureSession.ResourceGroups
                    .Define(azureResourceGroup.ResourceGroupName)
                    .WithRegion(azureResourceGroup.Region)
                    .CreateAsync()
                    .ConfigureAwait(false);

                return newGroup.Id;
            }
            else
            {
                var existingGroup = await azureSession.ResourceGroups
                    .GetByNameAsync(azureResourceGroup.ResourceGroupName)
                    .ConfigureAwait(false);

                return existingGroup.Id;
            }
        }
    }
}