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
    public sealed class ComponentSetActivity
    {
        private readonly IComponentRepository componentRepository;

        public ComponentSetActivity(IComponentRepository componentRepository)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }

        [FunctionName(nameof(ComponentSetActivity))]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var Component = await componentRepository
                .SetAsync(input.Component)
                .ConfigureAwait(false);

            return Component;
        }

        public struct Input
        {
            public Component Component { get; set; }
        }
    }
}
