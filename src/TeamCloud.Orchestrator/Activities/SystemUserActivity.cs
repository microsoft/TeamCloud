/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class SystemUserActivity
    {
        private readonly IAzureSessionService azureSessionService;

        public SystemUserActivity(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(SystemUserActivity))]
        public async Task<User> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var systemIdentity = await azureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            return new User()
            {
                Id = systemIdentity.ObjectId.ToString(),
                Role = OrganizationUserRole.None,
                UserType = UserType.System
            };
        }
    }
}
