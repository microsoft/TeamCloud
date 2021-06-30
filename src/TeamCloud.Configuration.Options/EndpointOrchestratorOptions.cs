/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Configuration.Options
{
    [Options("Endpoint:Orchestrator")]
    public sealed class EndpointOrchestratorOptions
    {
        public string Url { get; set; }

        public string AuthCode { get; set; }
    }
}
