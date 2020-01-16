/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Azure;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.Orchestrator.Options
{
    [Options]
    public class AzureSessionOptions : IAzureSessionOptions
    {
        private readonly AzureResourceManagerOptions azureRMOptions;

        public AzureSessionOptions(AzureResourceManagerOptions azureRMOptions)
        {
            this.azureRMOptions = azureRMOptions ?? throw new System.ArgumentNullException(nameof(azureRMOptions));
        }

        public string TenantId => azureRMOptions.TenantId;

        public string ClientId => azureRMOptions.ClientId;

        public string ClientSecret => azureRMOptions.ClientSecret;
    }
}
