/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public class CommandProviderActivity
    {
        private readonly ITeamCloudRepository teamCloudRepository;

        public CommandProviderActivity(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(CommandProviderActivity))]
        [RetryOptions(3)]
        public async Task<IEnumerable<IEnumerable<Provider>>> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var project = functionContext.GetInput<Project>();

            if (project is null)
            {
                return Enumerable.Repeat(teamCloud.Providers, 1);
            }
            else
            {
                return project.Type.Providers.Resolve(teamCloud);
            }
        }
    }
}
