/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ManagementGroups;
using Newtonsoft.Json.Linq;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Azure.Resources;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;
using TeamCloud.Serialization.Forms;

namespace TeamCloud.Adapters.AzureResourceManager
{
    public sealed class AzureResourceManagerAdapter : Adapter
    {

        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;

        public AzureResourceManagerAdapter(IServiceProvider serviceProvider, IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient, IAzureSessionService azureSessionService, IAzureResourceService azureResourceService)
            : base(serviceProvider, sessionClient, tokenClient)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        public override DeploymentScopeType Type
            => DeploymentScopeType.AzureResourceManager;

        public override async Task<string> GetInputDataSchemaAsync()
        {
            var json = await TeamCloudForm.GetDataSchemaAsync<AzureResourceManagerData>(true)
                .ConfigureAwait(false);

            await Task.WhenAll
            (
                EnhanceSubscriptionIdsAsync(),
                EnhanceManagementGroupIdAsync()

            ).ConfigureAwait(false);

            return json.ToString(Newtonsoft.Json.Formatting.None);

            async Task EnhanceSubscriptionIdsAsync()
            {
                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync()
                    .ConfigureAwait(false);

                var subscriptions = await session.Subscriptions
                    .ListAsync(loadAllPages: true)
                    .ConfigureAwait(false);

                if (subscriptions.Any() && json.TrySelectToken("properties.subscriptionIds.items", out var subscriptionIdsToken))
                {
                    subscriptionIdsToken["enum"] = new JArray(subscriptions.OrderBy(s => s.DisplayName).Select(s => s.SubscriptionId));
                    subscriptionIdsToken["enumNames"] = new JArray(subscriptions.OrderBy(s => s.DisplayName).Select(s => s.DisplayName));
                    ((JToken)subscriptionIdsToken.Parent.Parent)["uniqueItems"] = new JValue(true);
                }
            }

            async Task EnhanceManagementGroupIdAsync()
            {
                var client = await azureResourceService.AzureSessionService
                    .CreateClientAsync<ManagementGroupsAPIClient>()
                    .ConfigureAwait(false);

                var managementGroupPage = await client.ManagementGroups
                    .ListAsync()
                    .ConfigureAwait(false);

                var managementGroups = await managementGroupPage
                    .AsContinuousCollectionAsync(nextPageLink => client.ManagementGroups.ListNextAsync(nextPageLink))
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (managementGroups.Any() && json.TrySelectToken("properties.managementGroupId", out var managementGroupToken))
                {
                    managementGroupToken["enum"] = new JArray(managementGroups.OrderBy(mg => mg.DisplayName).Select(mg => mg.Id));
                    managementGroupToken["enumNames"] = new JArray(managementGroups.OrderBy(mg => mg.DisplayName).Select(mg => mg.DisplayName));
                }
            }
        }

        public override async Task<string> GetInputFormSchemaAsync()
        {
            var json = await TeamCloudForm.GetFormSchemaAsync<AzureResourceManagerData>()
                .ConfigureAwait(false);

            return json.ToString(Newtonsoft.Json.Formatting.None);
        }

        public override Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
            => azureSessionService.GetIdentityAsync().ContinueWith(identity => identity != null, TaskScheduler.Current);
    }
}
