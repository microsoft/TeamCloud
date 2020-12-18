/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    public partial class ProjectComponentsController : ApiController
    {
        [HttpPost("{componentId:componentId}/reset")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "ResetProjectComponent", Summary = "Reset a Project Component.")]
        [SwaggerResponse(StatusCodes.Status201Created, "Reset a Project Component. Returns the current version of the target Project Component.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Component with the provided componentId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Reset() => ExecuteAsync(new Func<User, Organization, Project, Component, Task<IActionResult>>(async (user, organization, project, component) =>
        {
            var command = new ComponentResetCommand(user, component);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        }));

        [HttpPost("{componentId:componentId}/clear")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "ClearProjectComponent", Summary = "Clear a Project Component.")]
        [SwaggerResponse(StatusCodes.Status201Created, "Clear a Project Component. Returns the current version of the target Project Component.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Component with the provided componentId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Clear() => ExecuteAsync(new Func<User, Organization, Project, Component, Task<IActionResult>>(async (user, organization, project, component) =>
        {
            var command = new ComponentClearCommand(user, component);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        }));
    }
}

