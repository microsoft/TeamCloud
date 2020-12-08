/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Deployment;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Templates.Subscription;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class OrganizationDeployActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;

        public OrganizationDeployActivity(IAzureDeploymentService azureDeploymentService)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new System.ArgumentNullException(nameof(azureDeploymentService));
        }

        [FunctionName(nameof(OrganizationDeployActivity))]
        [RetryOptions(3)]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var template = new OrganizationDeployTemplate();

            template.Parameters["organizationId"] = input.Organization.Id;
            template.Parameters["organizationSlug"] = input.Organization.Slug;

            var deployment = await azureDeploymentService
                .DeploySubscriptionTemplateAsync(template, Guid.Parse(input.Organization.SubscriptionId), input.Organization.Location)
                .ConfigureAwait(false);

            return deployment.ResourceId;
        }

        internal struct Input
        {
            public Organization Organization { get; set; }
        }
    }
}
