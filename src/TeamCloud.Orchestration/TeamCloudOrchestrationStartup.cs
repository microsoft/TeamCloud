using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using TeamCloud.Orchestration;

[assembly: FunctionsStartup(typeof(TeamCloudOrchestrationStartup))]

namespace TeamCloud.Orchestration
{
    public class TeamCloudOrchestrationStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

        }
    }
}
