/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Serialization;

[assembly: FunctionsStartup(typeof(TeamCloudOrchestrationStartup))]

namespace TeamCloud.Orchestration
{
    public class TeamCloudOrchestrationStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services
                .AddSingleton<IMessageSerializerSettingsFactory, MessageSerializerSettingsFactory>();
        }
    }
}
