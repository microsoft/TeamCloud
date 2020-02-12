using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Rest.Azure;

namespace TeamCloud.Azure.Resources
{
    public sealed class AzureSubscription : AzureResource
    {
        private static string GetResourceId(Guid subscriptionId)
            => new AzureResourceIdentifier(subscriptionId).ToString();

        internal AzureSubscription(Guid subscriptionId)
            : base(GetResourceId(subscriptionId))
        { }

        public AzureSubscription(Guid subscriptionId, IAzureResourceService azureResourceService)
            : base(GetResourceId(subscriptionId), azureResourceService)
        { }

        public override async Task<bool> ExistsAsync()
        {
            var session = AzureResourceService.AzureSessionService.CreateSession();

            try
            {
                _ = await session.Subscriptions
                    .GetByIdAsync(this.ResourceId.SubscriptionId.ToString())
                    .ConfigureAwait(false);

                return true;
            }
            catch (CloudException exc) when (exc.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
