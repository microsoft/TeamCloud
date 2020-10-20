/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Data.Validators;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Produces("application/json")]
    public class ProjectController : ApiController
    {
        private readonly IProjectTypeRepository projectTypeRepository;

        public ProjectController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IProjectTypeRepository projectTypeRepository)
            : base(userService, orchestrator, projectRepository)
        {
            this.projectTypeRepository = projectTypeRepository ?? throw new ArgumentNullException(nameof(projectTypeRepository));
        }

        private async Task<List<UserDocument>> ResolveUsersAsync(ProjectDefinition projectDefinition, string projectId)
        {
            var users = new List<UserDocument>();

            if (projectDefinition.Users?.Any() ?? false)
            {
                var tasks = projectDefinition.Users.Select(user => ResolveUserAndEnsureMembershipAsync(user, projectId));
                users = (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();
            }

            if (!users.Any(u => u.Id == UserService.CurrentUserId))
            {
                var currentUser = await UserService
                    .CurrentUserAsync()
                    .ConfigureAwait(false);

                currentUser.EnsureProjectMembership(projectId, ProjectUserRole.Owner);

                users.Add(currentUser);
            }

            return users;

            async Task<UserDocument> ResolveUserAndEnsureMembershipAsync(UserDefinition userDefinition, string projectId)
            {
                var user = await UserService
                    .ResolveUserAsync(userDefinition)
                    .ConfigureAwait(false);

                var role = user.Id == UserService.CurrentUserId ? ProjectUserRole.Owner : Enum.Parse<ProjectUserRole>(userDefinition.Role, true);
                user.EnsureProjectMembership(projectId, role, userDefinition.Properties);

                return user;
            }
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjects", Summary = "Gets all Projects.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Projects.", typeof(DataResult<List<Project>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var projects = await ProjectRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            var returnProjects = projects.Select(p => p.PopulateExternalModel()).ToList();

            return DataResult<List<Project>>
                .Ok(returnProjects)
                .ToActionResult();
        }


        [HttpGet("{projectNameOrId:projectNameOrId}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectByNameOrId", Summary = "Gets a Project by Name or ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a Project.", typeof(DataResult<Project>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the specified Name or ID was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get([FromRoute] string projectNameOrId) => EnsureProjectAsync(project =>
        {
            return DataResult<Project>
                .Ok(project.PopulateExternalModel())
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectCreate)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProject", Summary = "Creates a new Project.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Started creating the new Project. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project already exists with the name specified in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] ProjectDefinition projectDefinition)
        {
            if (projectDefinition is null)
                throw new ArgumentNullException(nameof(projectDefinition));

            var validation = new ProjectDefinitionValidator().Validate(projectDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var nameExists = await ProjectRepository
                .NameExistsAsync(projectDefinition.Name)
                .ConfigureAwait(false);

            if (nameExists)
                return ErrorResult
                    .Conflict($"A Project with name '{projectDefinition.Name}' already exists. Project names must be unique. Please try your request again with a unique name.")
                    .ToActionResult();

            var projectId = Guid.NewGuid().ToString();

            var users = await ResolveUsersAsync(projectDefinition, projectId)
                .ConfigureAwait(false);

            var project = new ProjectDocument
            {
                Id = projectId,
                Users = users,
                Name = projectDefinition.Name,
                Tags = projectDefinition.Tags,
                Properties = projectDefinition.Properties
            };

            if (!string.IsNullOrEmpty(projectDefinition.ProjectType))
            {
                project.Type = await projectTypeRepository
                    .GetAsync(projectDefinition.ProjectType)
                    .ConfigureAwait(false);

                if (project.Type is null)
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "projectType", Message = $"A Project Type with the ID '{projectDefinition.ProjectType}' could not be found in this TeamCloud Instance. Please try your request again with a valid Project Type ID for 'projectType'." })
                        .ToActionResult();
            }
            else
            {
                project.Type = await projectTypeRepository
                    .GetDefaultAsync()
                    .ConfigureAwait(false);

                if (project.Type is null)
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "projectType", Message = $"No value was provided for 'projectType' and there is no a default Project Type set for this TeamCloud Instance. Please try your request again with a valid Project Type ID for 'projectType'." })
                        .ToActionResult();
            }

            var currentUser = users.FirstOrDefault(u => u.Id == UserService.CurrentUserId);

            var command = new OrchestratorProjectCreateCommand(currentUser, project);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProjectDocument, Project>(command, Request)
                .ConfigureAwait(false);
        }


        [HttpDelete("{projectNameOrId:projectNameOrId}")]
        [Authorize(Policy = AuthPolicies.ProjectWrite)]
        [SwaggerOperation(OperationId = "DeleteProject", Summary = "Deletes a Project.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the specified Project. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the specified name or ID was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string projectNameOrId) => EnsureProjectAsync(async project =>
        {
            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectDeleteCommand(currentUser, project);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProjectDocument, Project>(command, Request)
                .ConfigureAwait(false);
        });
    }
}
