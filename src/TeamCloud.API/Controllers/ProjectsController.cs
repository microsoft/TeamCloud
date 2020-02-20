/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Data;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Produces("application/json")]
    public class ProjectsController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IProjectsRepositoryReadOnly projectsRepository;
        readonly IProjectTypesRepositoryReadOnly projectTypesRepository;

        public ProjectsController(UserService userService, Orchestrator orchestrator, IProjectsRepositoryReadOnly projectsRepository, IProjectTypesRepositoryReadOnly projectTypesRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };

        private async Task<List<User>> ResolveUsersAsync(ProjectDefinition projectDefinition)
        {
            if (projectDefinition.Users is null || !projectDefinition.Users.Any())
                return new List<User> { CurrentUser };

            var tasks = projectDefinition.Users.Select(user => userService.GetUserAsync(user));
            var users = await Task.WhenAll(tasks).ConfigureAwait(false);
            var owners = users.Where(user => user.Role.Equals(UserRoles.Project.Owner));

            var filteredUsers = users
                .Where(user => user.Role.Equals(UserRoles.Project.Member))
                .Except(owners, new UserComparer()) // filter out owners
                .Union(owners) // union members and owners
                .ToList();

            // ensure current user and owner role
            var currentUser = filteredUsers.FirstOrDefault(u => u.Id.Equals(CurrentUser.Id));

            if (currentUser is null)
                filteredUsers.Add(CurrentUser);
            else if (currentUser.Role != UserRoles.Project.Owner)
                currentUser.Role = UserRoles.Project.Owner;

            return filteredUsers;
        }


        [HttpGet]
        [Authorize(Policy = "projectRead")]
        [SwaggerOperation(OperationId = "GetProjects", Summary = "Gets all Projects.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Projects.", typeof(DataResult<List<Project>>))]
        public async Task<IActionResult> Get()
        {
            var projects = await projectsRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<Project>>
                .Ok(projects)
                .ActionResult();
        }


        [HttpGet("{projectId:guid}")]
        [Authorize(Policy = "projectRead")]
        [SwaggerOperation(OperationId = "GetProjectById", Summary = "Gets a Project by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a Project.", typeof(DataResult<Project>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the specified projectId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get(Guid projectId)
        {
            var project = await projectsRepository
                .GetAsync(projectId)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{projectId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            return DataResult<Project>
                .Ok(project)
                .ActionResult();
        }


        [HttpPost]
        [Authorize(Policy = "projectCreate")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProject", Summary = "Creates a new Project.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Started creating the new Project. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectDefinition specified in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project already exists with the name specified in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] ProjectDefinition projectDefinition)
        {
            var validation = new ProjectDefinitionValidator().Validate(projectDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var users = await ResolveUsersAsync(projectDefinition)
                .ConfigureAwait(false);

            var nameExists = await projectsRepository
                .NameExistsAsync(projectDefinition.Name)
                .ConfigureAwait(false);

            if (nameExists)
                return ErrorResult
                    .Conflict($"A Project with name '{projectDefinition.Name}' already exists. Project names must be unique. Please try your request again with a unique name.")
                    .ActionResult();

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Users = users,
                Name = projectDefinition.Name,
                Tags = projectDefinition.Tags
            };

            if (!string.IsNullOrEmpty(projectDefinition.ProjectType))
            {
                project.Type = await projectTypesRepository
                    .GetAsync(projectDefinition.ProjectType)
                    .ConfigureAwait(false);

                if (project.Type is null)
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "projectType", Message = $"A Project Type with the ID '{projectDefinition.ProjectType}' could not be found in this TeamCloud Instance. Please try your request again with a valid Project Type ID for 'projectType'." })
                        .ActionResult();
            }
            else
            {
                project.Type = await projectTypesRepository
                    .GetDefaultAsync()
                    .ConfigureAwait(false);

                if (project.Type is null)
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "projectType", Message = $"No value was provided for 'projectType' and there is no a default Project Type set for this TeamCloud Instance. Please try your request again with a valid Project Type ID for 'projectType'." })
                        .ActionResult();
            }

            var command = new ProjectCreateCommand(CurrentUser, project);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }


        [HttpDelete("{projectId:guid}")]
        [Authorize(Policy = "projectDelete")]
        [SwaggerOperation(OperationId = "DeleteProject", Summary = "Deletes a Project.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the specified Project. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the specified projectId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete(Guid projectId)
        {
            var project = await projectsRepository
                .GetAsync(projectId)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{projectId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            var command = new ProjectDeleteCommand(CurrentUser, project);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }
    }
}
