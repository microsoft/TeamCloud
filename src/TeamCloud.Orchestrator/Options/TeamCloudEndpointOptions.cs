/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Services;

namespace TeamCloud.Orchestrator.Options
{
    [Options]
    public sealed class TeamCloudEndpointOptions : IApiOptions, IFunctionsHost
    {
        private readonly EndpointApiOptions endpointApiOptions;
        private readonly EndpointPortalOptions endpointPortalOptions;
        private readonly EndpointOrchestratorOptions endpointOrchestratorOptions;

        public TeamCloudEndpointOptions(EndpointApiOptions endpointApiOptions, EndpointPortalOptions endpointPortalOptions, EndpointOrchestratorOptions endpointOrchestratorOptions)
        {
            this.endpointApiOptions = endpointApiOptions ?? throw new System.ArgumentNullException(nameof(endpointApiOptions));
            this.endpointPortalOptions = endpointPortalOptions ?? throw new System.ArgumentNullException(nameof(endpointPortalOptions));
            this.endpointOrchestratorOptions = endpointOrchestratorOptions ?? throw new System.ArgumentNullException(nameof(endpointOrchestratorOptions));
        }

        public string Api => endpointApiOptions.Url;

        public string Portal => endpointPortalOptions.Url;

        public string Orchestrator => endpointOrchestratorOptions.Url;

        string IApiOptions.Url => this.Api;

        string IFunctionsHost.HostUrl => this.Orchestrator;
    }
}
