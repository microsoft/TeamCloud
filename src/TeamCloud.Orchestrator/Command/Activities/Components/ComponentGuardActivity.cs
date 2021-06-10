/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.Components
{
    public sealed class ComponentGuardActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IAzureSessionService azureSessionService;

        public ComponentGuardActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IAzureSessionService azureSessionService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(ComponentGuardActivity))]
        public async Task<bool> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var component = context.GetInput<Input>().Component;

            try
            {
                var results = await Task.WhenAll(

                    GuardOrganizationAsync(component),
                    GuardProjectAsync(component)

                ).ConfigureAwait(false);

                return results.All(r => r);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Guard evaluation for component {component.Id} ({component.Slug}) in project {component.ProjectId} failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }

        private async Task<bool> GuardOrganizationAsync(Component component)
        {
            var tenantId = await azureSessionService
                .GetTenantIdAsync()
                .ConfigureAwait(false);

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            if (organization.ResourceState == ResourceState.Failed)
                throw new NotSupportedException($"Organization '{organization.Slug}' ended up in a Failed resource state.");

            return organization.ResourceState == ResourceState.Succeeded;
        }

        private async Task<bool> GuardProjectAsync(Component component)
        {
            var project = await projectRepository
                .GetAsync(component.Organization, component.ProjectId)
                .ConfigureAwait(false);

            if (project.ResourceState == ResourceState.Failed)
                throw new NotSupportedException($"Project '{project.Slug}' ended up in a Failed resource state.");

            return project.ResourceState == ResourceState.Succeeded;
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
