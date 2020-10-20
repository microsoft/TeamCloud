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
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Activities
{
    public class CommandProviderActivity
    {
        private readonly IProviderRepository providerRepository;

        public CommandProviderActivity(IProviderRepository providerRepository)
        {
            this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        }

        [FunctionName(nameof(CommandProviderActivity))]
        [RetryOptions(3)]
        public async Task<IEnumerable<IEnumerable<ProviderDocument>>> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var project = activityContext.GetInput<ProjectDocument>();

            if (project is null)
            {
                var providers = await providerRepository
                    .ListAsync()
                    .ToListAsync()
                    .ConfigureAwait(false);

                return Enumerable.Repeat(providers, 1);
            }
            else
            {
                var providers = await providerRepository
                    .ListAsync(project.Type.Providers.Select(p => p.Id))
                    .ToListAsync()
                    .ConfigureAwait(false);

                return project.Type.Providers.Resolve(providers);
            }
        }
    }
}
