using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Operations.Activities
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

            var input = context.GetInput<Input>();

            return await organizationRepository
                .SetAsync(input.Organization)
                .ConfigureAwait(false);
        }

        public struct Input
        {
            public Organization Organization { get; set; }
        }
    }
}
