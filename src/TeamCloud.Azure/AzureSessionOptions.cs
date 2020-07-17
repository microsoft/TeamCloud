/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure
{
    public interface IAzureSessionOptions
    {
        string TenantId { get; }

        string ClientId { get; }

        string ClientSecret { get; }
    }

    public sealed class AzureSessionOptions : IAzureSessionOptions
    {
        public static readonly IAzureSessionOptions Default
            = new AzureSessionOptions();

        private AzureSessionOptions()
        { }

        public string TenantId { get; internal set; } = default;

        public string ClientId { get; internal set; } = default;

        public string ClientSecret { get; internal set; } = default;
    }
}
