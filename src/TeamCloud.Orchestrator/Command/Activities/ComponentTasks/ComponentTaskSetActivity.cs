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

namespace TeamCloud.Orchestrator.Command.Activities.ComponentTasks
{
    public sealed class ComponentTaskSetActivity
    {
        private readonly IComponentTaskRepository componentTaskRepository;
        private readonly IComponentRepository componentRepository;

        public ComponentTaskSetActivity(IComponentTaskRepository componentTaskRepository, IComponentRepository componentRepository)
        {
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }

        [FunctionName(nameof(ComponentTaskSetActivity))]
        public async Task<ComponentTask> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var componentTask = context.GetInput<Input>().ComponentTask;

            componentTask = await componentTaskRepository
                .SetAsync(componentTask)
                .ConfigureAwait(false);

            return componentTask;
        }

        internal struct Input
        {
            public ComponentTask ComponentTask { get; set; }
        }
    }
}
