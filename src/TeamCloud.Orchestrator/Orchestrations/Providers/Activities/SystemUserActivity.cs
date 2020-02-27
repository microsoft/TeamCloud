using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Providers.Activities
{
    public class SystemUserActivity
    {
        private readonly IAzureSessionService azureSessionService;

        public SystemUserActivity(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new System.ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(SystemUserActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] object payload,
            ILogger log)
        {
            var systemIdentity = await azureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            return new User()
            {
                Id = systemIdentity.ObjectId,
                Role = UserRoles.TeamCloud.Admin
            };
        }

    }
}