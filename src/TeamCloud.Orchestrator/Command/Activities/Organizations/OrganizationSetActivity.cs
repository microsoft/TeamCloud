/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Activities.Organizations
{
    public sealed class OrganizationSetActivity
    {
        private readonly IOrganizationRepository organizationRepository;

        public OrganizationSetActivity(IOrganizationRepository organizationRepository)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        }

        [FunctionName(nameof(OrganizationSetActivity))]
        public async Task<Organization> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var organization = context.GetInput<Input>().Organization;

            organization.ResourceState = context.GetInput<Input>().ResourceState.GetValueOrDefault(organization.ResourceState);

            organization = await organizationRepository
                .SetAsync(organization)
                .ConfigureAwait(false);

            return organization;
        }

        internal struct Input
        {
            public Organization Organization { get; set; }

            public ResourceState? ResourceState { get; set; }
        }
    }
}
