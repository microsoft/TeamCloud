/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Activities.Components;

public sealed class ComponentIdentityActivity
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IDeploymentScopeRepository deploymentScopeRepository;
    private readonly IProjectRepository projectRepository;
    private readonly IAzureService azure;

    public ComponentIdentityActivity(IOrganizationRepository organizationRepository,
                                     IDeploymentScopeRepository deploymentScopeRepository,
                                     IProjectRepository projectRepository,
                                     IAzureService azure)
    {
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.azure = azure ?? throw new ArgumentNullException(nameof(azure));
    }

    [FunctionName(nameof(ComponentIdentityActivity))]
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

        if (string.IsNullOrEmpty(component.IdentityId))
        {
            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId)
                .ConfigureAwait(false);

            var project = await projectRepository
                .GetAsync(component.Organization, component.ProjectId)
                .ConfigureAwait(false);

            var projectResourceId = new ResourceIdentifier(project.ResourceId);

            var identity = await azure
                .GetUserAssignedIdentityAsync(projectResourceId.SubscriptionId, projectResourceId.ResourceGroupName, deploymentScope.Id)
                .ConfigureAwait(false);

            if (identity is null)
            {
                var location = await GetLocationAsync(component)
                    .ConfigureAwait(false);

                identity = await azure
                    .CreateUserAssignedIdentityAsync(projectResourceId.SubscriptionId, projectResourceId.ResourceGroupName, deploymentScope.Id, location)
                    .ConfigureAwait(false);
            }

            component.IdentityId = identity.Id;
        }

        return component;
    }

    private async Task<string> GetLocationAsync(Component component)
    {
        var tenantId = await azure
            .GetTenantIdAsync()
            .ConfigureAwait(false);

        var organization = await organizationRepository
            .GetAsync(tenantId, component.Organization)
            .ConfigureAwait(false);

        return organization.Location;
    }

    internal struct Input
    {
        public Component Component { get; set; }
    }
}
