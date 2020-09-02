/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public class UserGetActivity
    {
        private readonly IUserRepository usersRepository;

        public UserGetActivity(IUserRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        [FunctionName(nameof(UserGetActivity))]
        public async Task<UserDocument> RunActivity(
            [ActivityTrigger] string userId,
            ILogger log)
        {
            try
            {
                var user = await usersRepository
                    .GetAsync(userId)
                    .ConfigureAwait(false);

                return user;
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Failed to resolve user '{userId}': {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }
}
