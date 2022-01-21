/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.API.Services;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Options;

[Options]
public class TeamCloudEndpointOptions : IOrchestratorServiceOptions
{
    private readonly EndpointOrchestratorOptions endpointOrchestratorOptions;

    public TeamCloudEndpointOptions(EndpointOrchestratorOptions endpointOrchestratorOptions)
    {
        this.endpointOrchestratorOptions = endpointOrchestratorOptions;
    }

    public string Orchestrator => endpointOrchestratorOptions?.Url;

    string IOrchestratorServiceOptions.Url => endpointOrchestratorOptions?.Url;

    string IOrchestratorServiceOptions.AuthCode => endpointOrchestratorOptions?.AuthCode;
}
