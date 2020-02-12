using System;
using System.Threading.Tasks;

namespace TeamCloud.Azure.Resources
{
    public sealed class AzureResourceGroup : AzureResource
    {
        private static string GetResourceId(Guid subscriptionId, string resourceGroupName)
            => new AzureResourceIdentifier(subscriptionId, resourceGroupName).ToString();

        internal AzureResourceGroup(Guid subscriptionId, string resourceGroupName)
            : base(GetResourceId(subscriptionId, resourceGroupName))
        { }

        public AzureResourceGroup(Guid subscriptionId, string resourceGroupName, IAzureResourceService azureResourceService)
            : base(GetResourceId(subscriptionId, resourceGroupName), azureResourceService)
        { }

        public override Task<bool> ExistsAsync()
        {
            var session = AzureResourceService.AzureSessionService.CreateSession(this.ResourceId.SubscriptionId);

            return session.ResourceGroups.ContainAsync(this.ResourceId.ResourceGroup);
        }
    }
}