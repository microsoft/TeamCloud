/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Dynamitey.DynamicObjects;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Data;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Templates;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.Organizations;

public sealed class OrganizationDeployActivity
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IAzureSessionService azureSessionService;
    private readonly IAzureDeploymentService azureDeploymentService;
    private readonly IGraphService graphService;

    public OrganizationDeployActivity(IOrganizationRepository organizationRepository, IAzureSessionService azureSessionService, IAzureDeploymentService azureDeploymentService, IGraphService graphService)
    {
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
    }

    [FunctionName(nameof(OrganizationDeployActivity))]
    [RetryOptions(3)]
    public async Task<string> Run(
        [ActivityTrigger] IDurableActivityContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        try
        {
            var organization = context.GetInput<Input>().Organization;

            var template = new SharedResourcesTemplate();

            template.Parameters["organizationId"] = organization.Id;
            template.Parameters["organizationName"] = organization.DisplayName;
            template.Parameters["organizationSlug"] = organization.Slug;
            template.Parameters["organizationTags"] = organization.GetWellKnownTags();


            if (organization.Portal != PortalType.TeamCloud)
            {
                var servicePrincipalIdentifier = $"Portal/{organization.Id}";

                var servicePrincipal = await graphService
                    .GetServicePrincipalAsync(servicePrincipalIdentifier)
                    .ConfigureAwait(false);

                servicePrincipal ??= await graphService
                    .CreateServicePrincipalAsync(servicePrincipalIdentifier)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(servicePrincipal.Password))
                {
                    servicePrincipal = await graphService
                        .RefreshServicePrincipalAsync(servicePrincipalIdentifier)
                        .ConfigureAwait(false);
                }

                template.Parameters["portal"] = organization.Portal.ToString().ToLowerInvariant();
                template.Parameters["portalClientId"] = servicePrincipal.AppId;
                template.Parameters["portalClientSecret"] = servicePrincipal.Password;
            }

            var deployment = await azureDeploymentService
                .DeploySubscriptionTemplateAsync(template, Guid.Parse(organization.SubscriptionId), organization.Location)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }
        catch (Exception exc)
        {
            throw exc.AsSerializable();
        }
    }

    internal struct Input
    {
        public Organization Organization { get; set; }
    }
}
