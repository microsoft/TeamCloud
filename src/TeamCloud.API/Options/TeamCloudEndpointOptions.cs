/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.API.Services;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Options
{
    [Options]
    public class TeamCloudEndpointOptions : IOrchestratorOptions
    {
        private readonly EndpointOrchestratorOptions endpointOrchestratorOptions;

        public TeamCloudEndpointOptions(EndpointOrchestratorOptions endpointOrchestratorOptions)
        {
            this.endpointOrchestratorOptions = endpointOrchestratorOptions;
        }

        public string Orchestrator => endpointOrchestratorOptions?.Url;

        string IOrchestratorOptions.Url => endpointOrchestratorOptions?.Url;

        string IOrchestratorOptions.AuthCode => endpointOrchestratorOptions?.AuthCode;
    }
}
