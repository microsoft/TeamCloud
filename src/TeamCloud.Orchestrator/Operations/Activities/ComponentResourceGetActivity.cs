/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public class ComponentResourceGetActivity
    {
        private readonly IProjectRepository projectRepository;
        private readonly IProjectTemplateRepository projectTemplateRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentResourceGetActivity(IProjectRepository projectRepository, IProjectTemplateRepository projectTemplateRepository, IDeploymentScopeRepository deploymentScopeRepository, IAzureResourceService azureResourceService)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.projectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentResourceGetActivity))]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var component = context.GetInput<Component>();

            if (string.IsNullOrEmpty(component.ResourceId))
            {
                var project = await projectRepository
                    .GetAsync(component.Organization, component.ProjectId)
                    .ConfigureAwait(false);

                var projectTemplate = await projectTemplateRepository
                    .GetAsync(component.Organization, project.Template)
                    .ConfigureAwait(false);
            }

            return component.ResourceId;
        }

    }
}
