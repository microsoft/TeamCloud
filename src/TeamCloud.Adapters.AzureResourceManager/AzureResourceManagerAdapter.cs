/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.AzureResourceManager
{
    public sealed class AzureResourceManagerAdapter : Adapter
    {
        private readonly IAzureSessionService azureSessionService;

        public AzureResourceManagerAdapter(IServiceProvider serviceProvider, IAuthorizationSessionClient sessionClient, IAuthorizationTokenClient tokenClient, IAzureSessionService azureSessionService)
            : base(serviceProvider, sessionClient, tokenClient)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        public override bool Supports(DeploymentScope deploymentScope)
            => deploymentScope != null && deploymentScope.Type == DeploymentScopeType.AzureResourceManager;

        public override Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
            => azureSessionService.GetIdentityAsync().ContinueWith(identity => identity != null, TaskScheduler.Current);
    }
}
