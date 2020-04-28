/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using TeamCloud.Orchestration.Deployment;

[assembly: FunctionsStartup(typeof(TeamCloudOrchestrationDeploymentStartup))]

namespace TeamCloud.Orchestration.Deployment
{
    public class TeamCloudOrchestrationDeploymentStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
        }
    }
}
