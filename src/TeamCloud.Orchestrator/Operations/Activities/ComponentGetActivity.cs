/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentGetActivity
    {
        private readonly IComponentRepository componentRepository;

        public ComponentGetActivity(IComponentRepository componentRepository)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }

        [FunctionName(nameof(ComponentGetActivity))]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            return await componentRepository
                .GetAsync(input.ProjectId, input.Id)
                .ConfigureAwait(false);
        }

        public struct Input
        {
            public string Id { get; set; }

            public string ProjectId { get; set; }
        }
    }
}
