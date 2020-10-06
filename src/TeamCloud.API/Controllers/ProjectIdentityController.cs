/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/identity")]
    [Produces("application/json")]
    public class ProjectIdentitiesController : ApiController
    {
        public ProjectIdentitiesController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository)
            : base(userService, orchestrator, projectRepository)
        { }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectIdentityRead)]
        [SwaggerOperation(OperationId = "GetProjectIdentity", Summary = "Gets the ProjectIdentity for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns ProjectIdentity", typeof(DataResult<ProjectIdentity>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a ProjectIdentity was not found for the Project.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectAsync(project =>
        {
            if (project?.Identity is null)
                return ErrorResult
                    .NotFound($"A ProjectIdentity was not found for the Project '{project.Id}'.")
                    .ToActionResult();

            return DataResult<ProjectIdentity>
                .Ok(project.Identity)
                .ToActionResult();
        });
    }
}
