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

namespace TeamCloud.Orchestrator.Command.Activities.Components
{
    public sealed class ComponentEnsureTaggingActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentEnsureTaggingActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentEnsureTaggingActivity))]
        [RetryOptions(3)]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var component = context.GetInput<Input>().Component;

            try
            {
                if (AzureResourceIdentifier.TryParse(component.ResourceId, out var azureResourceIdentifier))
                {
                    var tenantId = (await azureResourceService.AzureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

                    var organization = await organizationRepository
                        .GetAsync(tenantId.ToString(), component.Organization, true)
                        .ConfigureAwait(false);

                    var project = await projectRepository
                        .GetAsync(component.Organization, component.ProjectId, true)
                        .ConfigureAwait(false);

                    var tags = organization.Tags
                        .Union(project.Tags)
                        .GroupBy(kvp => kvp.Key)
                        .ToDictionary(g => g.Key, g => g.First().Value);

                    if (string.IsNullOrEmpty(azureResourceIdentifier.ResourceGroup))
                    {
                        var subscription = await azureResourceService
                            .GetSubscriptionAsync(azureResourceIdentifier.SubscriptionId)
                            .ConfigureAwait(false);

                        if (subscription != null)
                            await subscription.SetTagsAsync(tags, true).ConfigureAwait(false);
                    }
                    else
                    {
                        var resourceGroup = await azureResourceService
                            .GetResourceGroupAsync(azureResourceIdentifier.SubscriptionId, azureResourceIdentifier.ResourceGroup)
                            .ConfigureAwait(false);

                        if (resourceGroup != null)
                            await resourceGroup.SetTagsAsync(tags, true).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception exc)
            {
                throw exc.AsSerializable();
            }

            return component;
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
