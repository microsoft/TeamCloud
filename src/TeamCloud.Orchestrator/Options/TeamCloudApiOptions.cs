/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Orchestrator.Services;

namespace TeamCloud.Orchestrator.Options
{
    [Options("Api")]
    public sealed class TeamCloudApiOptions : IApiOptions
    {
        public string Url { get; set; }
    }
}
