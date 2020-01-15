/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class AzureSubscriptionPoolSelectActivity
    {
        private readonly IAzureSessionFactory azureSessionFactory;

        public AzureSubscriptionPoolSelectActivity(IAzureSessionFactory azureSessionFactory)
        {
            this.azureSessionFactory = azureSessionFactory ?? throw new ArgumentNullException(nameof(azureSessionFactory));
        }

        [FunctionName(nameof(AzureSubscriptionPoolSelectActivity))]
        public async Task<string> RunActivity(
            [ActivityTrigger] TeamCloudInstance teamCloud)
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            
            // Ensure there is a SubscriptionPoolIds
            if (teamCloud?.Configuration?.Azure?.SubscriptionPoolIds == null)
                throw new ArgumentNullException(nameof(teamCloud.Configuration.Azure.SubscriptionPoolIds));

            // Ensure there is a SubscriptionPoolIds
            if (teamCloud.Configuration.Azure.SubscriptionPoolIds.Count == 0)
                throw new ArgumentException("There are no subscription IDs within the SubscriptionPoolIds list.", nameof(teamCloud.Configuration.Azure.SubscriptionPoolIds));

#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            // Determine which Subscription has the least amount of projects
            Dictionary<string, int> subscriptions = new Dictionary<string, int>();
            foreach (var subscriptionID in teamCloud.Configuration.Azure.SubscriptionPoolIds)
            {
                // Create instance to Azure instance
                var azureSession = azureSessionFactory.CreateSession(Guid.Parse(subscriptionID));

                // Store the count of all the resource groups in the subscription
                var list = await azureSession.ResourceGroups.ListAsync(true).ConfigureAwait(false);

                // Queue the count for the current subscription
                subscriptions.Add(subscriptionID, list.Count());
            }

            // Choose the subscription with least amount of resource groups (Item1 represents count of groups)
            var keyPair = subscriptions.OrderBy(s => s.Value).First();

            // Return the SubscriptionID of the subscription with the least groups
            return keyPair.Key;
        }
    }
}