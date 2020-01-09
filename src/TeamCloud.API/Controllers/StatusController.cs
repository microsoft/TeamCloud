/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly Orchestrator orchestrator;

        public StatusController(Orchestrator orchestrator)
        {
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        [Authorize(Policy = "admin")]
        [HttpGet("api/status/{instanceId:guid}")]
        public async Task<IActionResult> Get(Guid instanceId)
        {
            var result = await orchestrator
                .QueryAsync(instanceId, null)
                .ConfigureAwait(false);

            if (result is null) return new NotFoundResult();

            return new ObjectResult(result);
        }

        [Authorize(Policy = "projectRead")]
        [HttpGet("api/projects/{projectId:guid}/status/{commandId:guid}")]
        public async Task<IActionResult> Get(Guid projectId, Guid commandId)
        {
            var result = await orchestrator
                .QueryAsync(commandId, projectId)
                .ConfigureAwait(false);

            if (result is null) return new NotFoundResult();

            return new ObjectResult(result);
        }
    }
}
