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
        readonly Orchestrator orchestrator;
        readonly IProjectsRepository projectsRepository;

        public ProjectsController(Orchestrator orchestrator, IProjectsRepository projectsRepository)
        {
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
            if (projectDefinition is null) return new BadRequestResult();

            // This is a TeamCloud User, the user that called this api
            User user = null; // TODO: Get user from httpcontext?


            // these are Project Users
            List<User> users = new List<User>(); // TODO: projectDefinition.Users.Select(...)


            var project = new Project
            {
                Id = projectDefinition.Id,
                Name = projectDefinition.Name,
                Users = users,
                Tags = projectDefinition.Tags
            };

            var existingUser = project.Users.FirstOrDefault(u => u.Id == user.Id);

            if (existingUser != null)
            {
                // ensure Role is Owner
                // maybe merge tags
                // project.Users.Add(new User { Id = user.Id, Role = UserRoles.Project.Owner, Tags = user.Tags });
            }
            else
            {
                // project.Users.Add(new User { Id = user.Id, Role = UserRoles.Project.Owner, Tags = user.Tags });
            }


            var command = new ProjectCreateCommand(project);

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
        public void Delete(Guid projectId)
        {
            // TODO:
        }
    }
}
