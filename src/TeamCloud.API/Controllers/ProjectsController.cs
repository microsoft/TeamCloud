/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.API.Data;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize(Policy = "projectRead")]
    public class ProjectsController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IProjectsRepositoryReadOnly projectsRepository;

        public ProjectsController(UserService userService, Orchestrator orchestrator, IProjectsRepositoryReadOnly projectsRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        // GET: api/projects
        [HttpGet]
        public async IAsyncEnumerable<Project> Get()
        {
            var projects = projectsRepository
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
            var project = await projectsRepository
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
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = projectDefinition.Name,
                Users = await ResolveUsersAsync(projectDefinition).ConfigureAwait(false),
                Tags = projectDefinition.Tags
            };

            var command = new ProjectCreateCommand(CurrentUser, project);

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
            var project = await projectsRepository
                .GetAsync(projectId)
                .ConfigureAwait(false);

            if (project is null) return new NotFoundResult();

            var command = new ProjectDeleteCommand(CurrentUser, project);

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

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };

        private async Task<List<User>> ResolveUsersAsync(ProjectDefinition projectDefinition)
        {
            var tasks = projectDefinition.Users.Select(user => userService.GetUserAsync(user));
            var users = await Task.WhenAll(tasks).ConfigureAwait(false);
            var owners = users.Where(user => user.Role.Equals(UserRoles.Project.Owner));

            return users
                .Where(user => user.Role.Equals(UserRoles.Project.Member))
                .Except(owners, new UserComparer()) // filter out owners
                .Union(owners) // union members and owners
                .ToList();
        }
    }
}
