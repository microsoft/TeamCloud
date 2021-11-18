/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Templates;

namespace TeamCloud.Orchestrator.Command.Activities.Projects
{
    public sealed class ProjectDestroyActivity
    {
        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;

        public ProjectDestroyActivity(IProjectRepository projectRepository, IAzureResourceService azureResourceService)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ProjectDestroyActivity))]
        [RetryOptions(3)]
        public async Task Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var project = context.GetInput<Input>().Project;

            if (AzureResourceIdentifier.TryParse(project.ResourceId, out var resourceId ))
            {
                var resourceGroup = await azureResourceService
                    .GetResourceGroupAsync(resourceId.SubscriptionId, resourceId.ResourceGroup)
                    .ConfigureAwait(false);

                if (resourceGroup is not null)
                {
                    await resourceGroup
                        .DeleteAsync(true)
                        .ConfigureAwait(false);
                }

            }
        }

        internal struct Input
        {
            public Project Project { get; set; }
        }
    }
}
