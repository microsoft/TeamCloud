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
using TeamCloud.API.Data.Results;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/identity")]
    [Produces("application/json")]
    public class ProjectIdentitiesController : ControllerBase
    {
        readonly IProjectsRepository projectsRepository;

        public ProjectIdentitiesController(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        public string ProjectId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();


        [HttpGet]
        [Authorize(Policy = "projectIdentityRead")]
        [SwaggerOperation(OperationId = "GetProjectIdentity", Summary = "Gets the ProjectIdentity for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns ProjectIdentity", typeof(DataResult<ProjectIdentity>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a ProjectIdentity was not found for the Project.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            if (string.IsNullOrEmpty(ProjectId))
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the identifier '{ProjectId}' was not found in this TeamCloud instance.")
                    .ActionResult();

            if (project?.Identity is null)
                return ErrorResult
                    .NotFound($"A ProjectIdentity was not found for the Project '{ProjectId}'.")
                    .ActionResult();

            return DataResult<ProjectIdentity>
                .Ok(project.Identity)
                .ActionResult();
        }
    }
}
