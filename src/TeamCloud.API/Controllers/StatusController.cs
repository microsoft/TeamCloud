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
        [HttpGet("api/status/{commandId:guid}")]
        public async Task<IActionResult> Get(Guid commandId)
        {
            var result = await orchestrator
                .QueryAsync(commandId, null)
                .ConfigureAwait(false);

            return GetStatusResult(result);
        }

        [Authorize(Policy = "projectRead")]
        [HttpGet("api/projects/{projectId:guid}/status/{commandId:guid}")]
        public async Task<IActionResult> Get(Guid projectId, Guid commandId)
        {
            var result = await orchestrator
                .QueryAsync(commandId, projectId)
                .ConfigureAwait(false);

            return GetStatusResult(result);
        }

        private IActionResult GetStatusResult(ICommandResult result)
        {
            if (result is null)
                return ErrorResult
                    .NotFound($"A status for the provided Tracking Id was not found.")
                    .ActionResult();

            result.Links.TryGetValue("status", out var status);

            switch (result.RuntimeStatus)
            {
                case CommandRuntimeStatus.Completed:

                    if (result.Links.TryGetValue("location", out var location))
                    {
                        // return 302 (found) with location to resource
                        Response.Headers.Add("Location", location);
                        return StatusResult
                            .Success(result.CommandId.ToString(), location)
                            .ActionResult();
                    }

                    // no resource location (i.e. DELETE command) return 200 (ok)
                    return StatusResult
                        .Success(result.CommandId.ToString())
                        .ActionResult();

                case CommandRuntimeStatus.Running:
                case CommandRuntimeStatus.ContinuedAsNew:
                case CommandRuntimeStatus.Pending:

                    // command is in an active state, return 202 (accepted) so client can poll
                    return StatusResult
                        .Accepted(result.CommandId.ToString(), status, result.RuntimeStatus.ToString(), result.CustomStatus)
                        .ActionResult();

                case CommandRuntimeStatus.Canceled:
                case CommandRuntimeStatus.Terminated:
                case CommandRuntimeStatus.Failed:

                    return ErrorResult
                        .ServerError(result.Exceptions, result.CommandId.ToString())
                        .ActionResult();

                default: // TODO: this probably isn't right as a default

                    if (result.Exceptions?.Any() ?? false)
                        return ErrorResult
                            .ServerError(result.Exceptions, result.CommandId.ToString())
                            .ActionResult();

                    return StatusResult
                        .Ok(result.CommandId.ToString(), result.RuntimeStatus.ToString(), result.CustomStatus)
                        .ActionResult();
            }
        }
    }
}
