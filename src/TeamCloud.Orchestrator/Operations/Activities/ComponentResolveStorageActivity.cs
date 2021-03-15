/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentResolveStorageActivity
    {
        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentResolveStorageActivity(IProjectRepository projectRepository, IAzureResourceService azureResourceService)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentResolveStorageActivity))]
        [RetryOptions(3)]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                var component = context.GetInput<Input>().Component;

                if (!AzureResourceIdentifier.TryParse(component.StorageId, out var _))
                {
                    var project = await projectRepository
                        .GetAsync(component.Organization, component.ProjectId)
                        .ConfigureAwait(false);

                    if (AzureResourceIdentifier.TryParse(project?.ResourceId, out var projectResourceId))
                    {
                        var projectResourceGroup = await azureResourceService
                            .GetResourceGroupAsync(projectResourceId.SubscriptionId, projectResourceId.ResourceGroup)
                            .ConfigureAwait(false);

                        var projectStorageResource = await projectResourceGroup
                            .GetResourcesByTypeAsync("Microsoft.Storage/storageAccounts")
                            .SingleAsync()
                            .ConfigureAwait(false);

                        component.StorageId = projectStorageResource.ResourceId.ToString();
                    }
                }

                return component;
            }
            catch (Exception exc)
            {
                throw exc.AsSerializable();
            }
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
