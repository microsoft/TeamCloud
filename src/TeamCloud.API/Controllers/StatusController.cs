/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.API.Data;
using TeamCloud.API.Services;
using TeamCloud.Model.Commands;

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

            if (result is null)
                return StatusResult.NotFound().ActionResult();

            return GetStatusResult(result).ActionResult();
        }

        [Authorize(Policy = "projectRead")]
        [HttpGet("api/projects/{projectId:guid}/status/{commandId:guid}")]
        public async Task<IActionResult> Get(Guid projectId, Guid commandId)
        {
            var result = await orchestrator
                .QueryAsync(commandId, projectId)
                .ConfigureAwait(false);

            if (result is null)
                return new NotFoundResult();

            return GetStatusResult(result).ActionResult();
        }

        private StatusResult GetStatusResult(ICommandResult result)
        {
            result.Links.TryGetValue("location", out var location);
            result.Links.TryGetValue("status", out var status);

            StatusResult statusResult = null;

            if (result.RuntimeStatus == CommandRuntimeStatus.Completed && !string.IsNullOrEmpty(location))
            {
                Response.Headers.Add("Location", location);
                statusResult = StatusResult.Success(location);
            }
            else if (result.RuntimeStatus.IsActive() && !string.IsNullOrEmpty(status))
            {
                statusResult = StatusResult.Accepted(status, result.RuntimeStatus.ToString(), result.CustomStatus);
            }
            else if (result.RuntimeStatus.IsStopped())
            {
                statusResult = StatusResult.Ok(result.RuntimeStatus.ToString(), result.CustomStatus);
            }
            else if (result.RuntimeStatus == CommandRuntimeStatus.Failed)
            {
                statusResult = StatusResult.Failed();
            }

            statusResult ??= StatusResult.Ok(result.RuntimeStatus.ToString(), result.CustomStatus);

            if (result.Exceptions?.Any() ?? false)
                statusResult.Errors = result.Exceptions.Select(e => new ResultError { Message = e.Message }).ToList();

            return statusResult;
        }
    }
}
