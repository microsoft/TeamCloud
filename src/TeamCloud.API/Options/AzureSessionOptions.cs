/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Azure;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Options
{
    [Options]
    public class AzureSessionOptions : IAzureSessionOptions
    {
        private readonly AzureRMOptions azureRMOptions;

        public AzureSessionOptions(AzureRMOptions azureRMOptions)
        {
            this.azureRMOptions = azureRMOptions ?? throw new ArgumentNullException(nameof(azureRMOptions));
        }

        public string TenantId => azureRMOptions.TenantId;

        public string ClientId => azureRMOptions.ClientId;

        public string ClientSecret => azureRMOptions.ClientSecret;
    }
}
