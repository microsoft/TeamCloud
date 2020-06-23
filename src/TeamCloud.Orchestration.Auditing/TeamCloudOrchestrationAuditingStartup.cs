/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using TeamCloud.Orchestration.Auditing;

[assembly: FunctionsStartup(typeof(TeamCloudOrchestrationAuditingStartup))]

namespace TeamCloud.Orchestration.Auditing
{
    public class TeamCloudOrchestrationAuditingStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
        }
    }
}
