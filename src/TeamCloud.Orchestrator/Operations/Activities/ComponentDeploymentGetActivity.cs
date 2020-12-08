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

        public ComponentDeploymentGetActivity(IComponentDeploymentRepository componentDeploymentRepository)
        {
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
        }

        [FunctionName(nameof(ComponentDeploymentGetActivity))]
        public async Task<ComponentDeployment> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var componentDeployment = await componentDeploymentRepository
                .GetAsync(input.ComponentId, input.Id)
                .ConfigureAwait(false);

            return componentDeployment;
        }

        internal struct Input
        {
            public string Id { get; set; }

            public string ComponentId { get; set; }
        }
    }
}
