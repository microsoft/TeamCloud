/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.API.Data
{
    public class SetTeamCloudInstanceTrigger
    {
        readonly ITeamCloudRepository teamCloudRepository;

        public SetTeamCloudInstanceTrigger(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [FunctionName(nameof(SetTeamCloudInstanceTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "data/teamCloudInstance")] TeamCloudInstanceDocument teamCloudInstance
            /* ILogger log */)
        {
            if (teamCloudInstance is null)
                throw new ArgumentNullException(nameof(teamCloudInstance));

            var newTeamCloudInstance = await teamCloudRepository
                .SetAsync(teamCloudInstance)
                .ConfigureAwait(false);

            return new OkObjectResult(newTeamCloudInstance);
        }
    }
}
