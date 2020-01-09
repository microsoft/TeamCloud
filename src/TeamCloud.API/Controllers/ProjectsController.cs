/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.Data;
using TeamCloud.Model;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize(Policy = "projectRead")]
    public class ProjectsController : ControllerBase
    {
        private User currentUser = new User
        {
            Id = Guid.Parse("bc8a62dc-c327-4418-a004-77c85c3fb488"),
            Role = UserRoles.TeamCloud.Admin
        };

        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IProjectsContainer projectsContainer;

        public ProjectsController(UserService userService, Orchestrator orchestrator, IProjectsContainer projectsContainer)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsContainer = projectsContainer ?? throw new ArgumentNullException(nameof(projectsContainer));
        }

        // GET: api/projects
        [HttpGet]
        public async IAsyncEnumerable<Project> Get()
        {
            var projects = projectsContainer
                .ListAsync();

            await foreach (var project in projects)
            {
                yield return project;
            }
        }

        // GET: api/projects/{projectId}
        [HttpGet("{projectId:guid}")]
        public async Task<IActionResult> Get(Guid projectId)
        {
            var project = await projectsContainer
                .GetAsync(projectId)
                .ConfigureAwait(false);

            return project is null
                ? (IActionResult)new NotFoundResult()
                : new OkObjectResult(project);
        }

        // POST: api/projects
        [HttpPost]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Post([FromBody] ProjectDefinition projectDefinition)
        {
            if (projectDefinition is null) return new BadRequestResult();

            var projectUsers = await GetUsersForNewProject(projectDefinition);

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = projectDefinition.Name,
                Users = projectUsers,
                Tags = projectDefinition.Tags
            };

            var command = new ProjectCreateCommand(currentUser, project);

            var commandResult = await orchestrator
                .InvokeAsync<Project>(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
            {
                return new AcceptedResult(statusUrl, commandResult);
            }
            else
            {
                return new OkObjectResult(commandResult);
            }
        }

        // PUT: api/projects
        [HttpPut]
        public void Put([FromBody] Project project)
        {
            // TODO:
        }

        // DELETE: api/projects/{projectId}
        [HttpDelete("{projectId:guid}")]
        [Authorize(Policy = "projectDelete")]
        public async Task<IActionResult> Delete(Guid projectId)
        {
            var project = await projectsContainer
                .GetAsync(projectId)
                .ConfigureAwait(false);

            if (project is null) return new NotFoundResult();

            var command = new ProjectDeleteCommand(currentUser, project);

            var commandResult = await orchestrator
                .InvokeAsync<Project>(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
            {
                return new AcceptedResult(statusUrl, commandResult);
            }
            else
            {
                return new OkObjectResult(commandResult);
            }
        }


        private async Task<List<User>> GetUsersForNewProject(ProjectDefinition projectDefinition)
        {
            var projectUserTasks = projectDefinition.Users.Select(u => userService.GetUser(u));

            var projectUsers = (await Task.WhenAll(projectUserTasks)).ToList();

            var currentProjectUser = projectUsers.FirstOrDefault(u => u.Id == currentUser.Id);

            if (currentProjectUser is null)
            {
                projectUsers.Append(new User
                {
                    Id = currentUser.Id,
                    Role = UserRoles.Project.Owner,
                    Tags = currentUser.Tags
                });
            }
            else if (currentProjectUser.Role != UserRoles.Project.Owner)
            {
                currentProjectUser.Role = UserRoles.Project.Owner;

                // TODO:
                // Should we merge the tags of the currentUser (TeamCloud User)?
                // Should we merge the tags of ALL users that are also TeamCloud Users?
            }

            return projectUsers;
        }
    }
}
