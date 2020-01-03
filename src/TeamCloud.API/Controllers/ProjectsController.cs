/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
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
        readonly IProjectsContainer projectsContainer;

        public ProjectsController(Orchestrator orchestrator, IProjectsContainer projectsContainer)
        {
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

            var command = new ProjectCreateCommand(projectDefinition);

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
