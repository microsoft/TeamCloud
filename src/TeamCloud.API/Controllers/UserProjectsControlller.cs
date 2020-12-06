/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class UserProjectsController : ApiController
    {
        private readonly IProjectRepository projectRepository;

        public UserProjectsController(IProjectRepository projectRepository) : base()
        {
            this.projectRepository = projectRepository ?? throw new System.ArgumentNullException(nameof(projectRepository));
        }


        [HttpGet("orgs/{org}/users/{userId:userId}/projects")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetUserProjects", Summary = "Gets all Projects for a User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all User Projects", typeof(DataResult<List<Project>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the provided userId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureUserAsync(async user =>
        {
            var projectIds = user.ProjectMemberships.Select(pm => pm.ProjectId);

            if (!projectIds.Any())
                return DataResult<List<Project>>
                    .Ok(new List<Project>())
                    .ToActionResult();

            var projects = await projectRepository
                .ListAsync(OrgId, projectIds)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<Project>>
                .Ok(projects)
                .ToActionResult();
        });


        [HttpGet("orgs/{org}/me/projects")] // TODO: change to users/orgs/{org}/projects
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetUserProjectsMe", Summary = "Gets all Projects for a User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all User Projects", typeof(DataResult<List<Project>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the provided userId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> GetMe() => EnsureCurrentUserAsync(async user =>
        {
            var projectIds = user.ProjectMemberships.Select(pm => pm.ProjectId);

            if (!projectIds.Any())
                return DataResult<List<Project>>
                    .Ok(new List<Project>())
                    .ToActionResult();

            var projects = await projectRepository
                .ListAsync(OrgId, projectIds)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<Project>>
                .Ok(projects)
                .ToActionResult();
        });
    }
}
