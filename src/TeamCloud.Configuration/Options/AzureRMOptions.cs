/**
*  Copyright (c) Microsoft Corporation.
*  Licensed under the MIT License.
*/

namespace TeamCloud.Configuration.Options
{
    [Options("AzureRM")]
    public class AzureRMOptions
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string TenantId { get; set; }
    }
}
