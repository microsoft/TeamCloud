/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Orchestrator.Services;

namespace TeamCloud.Orchestrator.Options
{
    [Options]
    public sealed class TeamCloudEndpointOptions : IApiOptions
    {
        private readonly EndpointApiOptions endpointApiOptions;
        private readonly EndpointPortalOptions endpointPortalOptions;

        public TeamCloudEndpointOptions(EndpointApiOptions endpointApiOptions, EndpointPortalOptions endpointPortalOptions)
        {
            this.endpointApiOptions = endpointApiOptions;
            this.endpointPortalOptions = endpointPortalOptions;
        }

        public string Api => endpointApiOptions?.Url;

        public string Portal => endpointPortalOptions?.Url;

        string IApiOptions.Url => this.Api;
    }
}
