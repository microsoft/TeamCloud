/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Options
{
    [Options("Orchestrator")]
    public class OrchestratorOptions : IOrchestratorOptions
    {
        public string Url { get; set; }

        public string AuthCode { get; set; }
    }
}
