using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentDeploymentSetActivity
    {
        private readonly IComponentDeploymentRepository componentDeploymentRepository;
        private readonly IComponentRepository componentRepository;

        public ComponentDeploymentSetActivity(IComponentDeploymentRepository componentDeploymentRepository, IComponentRepository componentRepository)
        {
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }

        [FunctionName(nameof(ComponentDeploymentSetActivity))]
        public async Task<ComponentDeployment> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var componentDeployment = context.GetInput<Input>().ComponentDeployment;

            if (string.IsNullOrEmpty(componentDeployment.StorageId))
            {
                var component = await componentRepository
                    .GetAsync(componentDeployment.ProjectId, componentDeployment.ComponentId)
                    .ConfigureAwait(false);

                componentDeployment.StorageId = component.StorageId;
            }

            componentDeployment = await componentDeploymentRepository
                .SetAsync(componentDeployment)
                .ConfigureAwait(false);

            return componentDeployment;
        }

        internal struct Input
        {
            public ComponentDeployment ComponentDeployment { get; set; }
        }
    }
}
