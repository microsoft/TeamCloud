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
        readonly UserService userService;
        readonly IUserRepository usersRepository;
        readonly IProjectRepository projectsRepository;

        public UserProjectsController(UserService userService, IUserRepository usersRepository, IProjectRepository projectsRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        public string UserId
            => RouteData.Values.GetValueOrDefault(nameof(UserId), StringComparison.OrdinalIgnoreCase)?.ToString();


        [HttpGet("api/users/{userId:guid}/projects")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetUserProjects", Summary = "Gets all Projects for a User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all User Projects", typeof(DataResult<List<Project>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the provided userId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            if (string.IsNullOrEmpty(UserId))
                return ErrorResult
                    .BadRequest($"User Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var user = await usersRepository
                .GetAsync(UserId)
                .ConfigureAwait(false);

            if (user is null)
                return ErrorResult
                    .NotFound($"The specified User could not be found in this TeamCloud Instance.")
                    .ToActionResult();

            var projectIds = user.ProjectMemberships.Select(pm => pm.ProjectId);

            if (!projectIds.Any())
                return DataResult<List<Project>>
                    .Ok(new List<Project>())
                    .ToActionResult();

            var projectDocuments = await projectsRepository
                .ListAsync(projectIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var projects = projectDocuments.Select(p => p.PopulateExternalModel()).ToList();

            return DataResult<List<Project>>
                .Ok(projects)
                .ToActionResult();
        }

        [HttpGet("api/me/projects")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetUserProjectsMe", Summary = "Gets all Projects for a User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all User Projects", typeof(DataResult<List<Project>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the provided userId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> GetMe()
        {
            var me = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            if (me is null)
                return ErrorResult
                    .NotFound($"A User matching the current authenticated user was not found in this TeamCloud instance.")
                    .ToActionResult();

            var projectIds = me.ProjectMemberships.Select(pm => pm.ProjectId);

            if (!projectIds.Any())
                return DataResult<List<Project>>
                    .Ok(new List<Project>())
                    .ToActionResult();

            var projectDocuments = await projectsRepository
                .ListAsync(projectIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var projects = projectDocuments.Select(p => p.PopulateExternalModel()).ToList();

            return DataResult<List<Project>>
                .Ok(projects)
                .ToActionResult();
        }
    }
}
