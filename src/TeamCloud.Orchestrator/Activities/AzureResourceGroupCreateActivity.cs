using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator.Activities
{
    public static class AzureResourceGroupCreateActivity
    {
        [FunctionName(nameof(AzureResourceGroupCreateActivity))]
        public async static Task<Guid> RunActivity(
            [ActivityTrigger] AzureResourceGroup azureResourceGroup)
        {
            IAzure azure = null; // TODO get a live authenticed instance


            // TODO move this azure authentication to singleton somewhere else so its global to all activities
            // start
            const string client = "";
            const string key = "";
            const string tenant_id = "";
            const string subscription_id = "";
            var creds = new AzureCredentialsFactory().FromServicePrincipal(client, key, tenant_id, AzureEnvironment.AzureGlobalCloud);
            azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(creds).WithSubscription(subscription_id);
            // end


            if (azureResourceGroup == null)
                throw new ArgumentNullException(nameof(azureResourceGroup));
            if (string.IsNullOrWhiteSpace(azureResourceGroup.ResourceGroupName))
                throw new ArgumentNullException(nameof(azureResourceGroup.ResourceGroupName));
            if (azureResourceGroup.Region == null)
                throw new ArgumentNullException(nameof(azureResourceGroup.Region));

            if (await azure.ResourceGroups.ContainAsync(azureResourceGroup.ResourceGroupName) == false)
            {
                IResourceGroup newGroup = await azure.ResourceGroups
                    .Define(azureResourceGroup.ResourceGroupName)
                    .WithRegion(azureResourceGroup.Region)
                    .CreateAsync();

                return Guid.Parse(newGroup.Id);
            }
            else
            {
                IResourceGroup existingGroup = await azure.ResourceGroups.GetByNameAsync(azureResourceGroup.ResourceGroupName);
                return Guid.Parse(existingGroup.Id);
            }
        }
    }
}