using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Templates.ResourceGroup;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class EnvironmentDeployActivity
    {
        private readonly IProjectRepository projectRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IAzureDeploymentService azureDeploymentService;

        public EnvironmentDeployActivity(IProjectRepository projectRepository, IComponentTemplateRepository componentTemplateRepository, IAzureDeploymentService azureDeploymentService)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(EnvironmentDeployActivity))]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var componentTemplate = await componentTemplateRepository
                .GetAsync(input.Component.Organization, input.Component.ProjectId, input.Component.TemplateId)
                .ConfigureAwait(false);

            var template = new EnvironmentDeployTemplate();

            template.Parameters["environmentId"] = input.Component.ResourceId;
            template.Parameters["environmentTemplatePath"] = componentTemplate.Folder;
            template.Parameters["environmentTemplateRepository"] = componentTemplate.Repository.Url;
            template.Parameters["environmentTemplateRevision"] = componentTemplate.Repository.Ref;
            template.Parameters["environmentTemplateParameters"] = JsonConvert.DeserializeObject(input.Component.InputJson);
            template.Parameters["deploymentRunner"] = $"markusheiliger/tcrunner-arm";
            template.Parameters["deploymentIdentity"] = input.Component.IdentityId;

            var componentResourceId = AzureResourceIdentifier.Parse(input.Component.ResourceId);

            var deployment = await azureDeploymentService
                .DeployResourceGroupTemplateAsync(template, componentResourceId.SubscriptionId, componentResourceId.ResourceGroup, input.Complete)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }

        public struct Input
        {
            public Component Component { get; set; }

            public bool Complete { get; set; }
        }
    }
}
