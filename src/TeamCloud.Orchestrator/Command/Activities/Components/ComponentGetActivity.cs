/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.Components
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

            try
            {
                var component = await componentRepository
                    .GetAsync(input.ProjectId, input.ComponentId)
                    .ConfigureAwait(false);

                return component;
            }
            catch(Exception exc)
            {
                throw exc.AsSerializable();
            }
        }

        internal struct Input
        {
            public string ComponentId { get; set; }

            public string ProjectId { get; set; }
        }
    }
}
