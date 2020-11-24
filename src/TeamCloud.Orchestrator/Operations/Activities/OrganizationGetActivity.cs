using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class OrganizationGetActivity
    {
        private readonly IOrganizationRepository organizationRepository;

        public OrganizationGetActivity(IOrganizationRepository organizationRepository)
        {
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
                .GetAsync(input.Tenant, input.Id)
                .ConfigureAwait(false);
        }

        public struct Input
        {
            public string Id { get; set; }

            public string Tenant { get; set; }
        }
    }
}
