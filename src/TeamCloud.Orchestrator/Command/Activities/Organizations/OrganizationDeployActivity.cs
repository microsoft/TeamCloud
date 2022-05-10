/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Data;
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

    public OrganizationDeployActivity(IOrganizationRepository organizationRepository, IAzureSessionService azureSessionService, IAzureDeploymentService azureDeploymentService)
    {
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
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
            template.Parameters["organizationName"] = organization.Slug;
            template.Parameters["organizationTags"] = organization.GetWellKnownTags();


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
