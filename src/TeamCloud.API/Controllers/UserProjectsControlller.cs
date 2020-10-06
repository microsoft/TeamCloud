/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
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
    [Produces("application/json")]
    public class UserProjectsController : ApiController
    {
        public UserProjectsController(UserService userService, Orchestrator orchestrator, IUserRepository userRepository, IProjectRepository projectRepository)
            : base(userService, orchestrator, projectRepository, userRepository)
        { }


        [HttpGet("api/users/{userId:guid}/projects")]
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

            var projectDocuments = await ProjectRepository
                .ListAsync(projectIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var projects = projectDocuments.Select(p => p.PopulateExternalModel()).ToList();

            return DataResult<List<Project>>
                .Ok(projects)
                .ToActionResult();
        });


        [HttpGet("api/me/projects")]
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

            var projectDocuments = await ProjectRepository
                .ListAsync(projectIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var projects = projectDocuments.Select(p => p.PopulateExternalModel()).ToList();

            return DataResult<List<Project>>
                .Ok(projects)
                .ToActionResult();
        });
    }
}
