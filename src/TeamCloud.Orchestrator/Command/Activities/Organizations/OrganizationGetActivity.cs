/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Activities.Organizations
{
    public sealed class OrganizationGetActivity
    {
        private readonly IAzureSessionService azureSessionService;
        private readonly IOrganizationRepository organizationRepository;

        public OrganizationGetActivity(IAzureSessionService azureSessionService, IOrganizationRepository organizationRepository)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        }

        [FunctionName(nameof(OrganizationGetActivity))]
        public async Task<Organization> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            return await organizationRepository
                .GetAsync(azureSessionService.Options.TenantId, input.Id)
                .ConfigureAwait(false);
        }

        internal struct Input
        {
            public string Id { get; set; }
        }
    }
}
