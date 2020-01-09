/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.Data;
using TeamCloud.Model;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/config")]
    [Authorize(Policy = "admin")]
    public class TeamCloudController : ControllerBase
    {
        readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudController(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        // GET: api/config
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            return teamCloudInstance is null
                ? (IActionResult)new NotFoundResult()
                : new OkObjectResult(teamCloudInstance.Configuration);
        }

        // POST: api/config
        [HttpPost]
        public void Post([FromBody] TeamCloudConfiguraiton teamCloudConfiguraiton)
        {
            /* TODO:
             *
             * - Change the input to a file upload
             * - This will be in the form of a yaml file (see: https://github.com/microsoft/TeamCloud/blob/master/docs/teamcloud.yaml)
             * - Possibly save (cache) the file in storage
             * - Parse the file into the TeamCloudConfiguraiton
             * - ...
             */
        }
    }
}
