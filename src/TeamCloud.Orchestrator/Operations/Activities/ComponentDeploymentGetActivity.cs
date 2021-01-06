using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentDeploymentGetActivity
    {
        private readonly IComponentDeploymentRepository componentDeploymentRepository;
        private readonly IComponentRepository componentRepository;

        public ComponentDeploymentGetActivity(IComponentDeploymentRepository componentDeploymentRepository, IComponentRepository componentRepository)
        {
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }

        [FunctionName(nameof(ComponentDeploymentGetActivity))]
        public async Task<ComponentDeployment> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var componentDeployment = await componentDeploymentRepository
                .GetAsync(input.ComponentId, input.ComponentDeploymentId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(componentDeployment.StorageId))
            {
                var component = await componentRepository
                    .GetAsync(componentDeployment.ProjectId, componentDeployment.ComponentId)
                    .ConfigureAwait(false);

                componentDeployment.StorageId = component.StorageId;
            }

            return componentDeployment;
        }

        internal struct Input
        {
            public string ComponentDeploymentId { get; set; }

            public string ComponentId { get; set; }
        }
    }
}
