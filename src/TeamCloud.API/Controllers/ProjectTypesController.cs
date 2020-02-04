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
using TeamCloud.API.Data;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Authorize(Policy = "admin")]
    [Route("api/projectTypes")]
    public class ProjectTypesController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;
        readonly IProjectTypesRepositoryReadOnly projectTypesRepository;

        public ProjectTypesController(UserService userService, Orchestrator orchestrator, ITeamCloudRepositoryReadOnly teamCloudRepository, IProjectTypesRepositoryReadOnly projectTypesRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };

        [HttpGet]
        public async IAsyncEnumerable<ProjectType> Get()
        {
            var projectTypes = projectTypesRepository
                .ListAsync()
                .ConfigureAwait(false);

            await foreach (var projectType in projectTypes)
                yield return projectType;
        }

        [HttpGet("{projectTypeId}")]
        public async Task<IActionResult> Get(string projectTypeId)
        {
            var projectType = await projectTypesRepository
                .GetAsync(projectTypeId)
                .ConfigureAwait(false);

            if (projectType is null)
                return new NotFoundResult();

            return new OkObjectResult(projectType);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProjectType projectType)
        {
            var existingProjectType = await projectTypesRepository
                .GetAsync(projectType.Id)
                .ConfigureAwait(false);

            if (existingProjectType != null)
                return new ConflictObjectResult($"A ProjectType with id '{projectType.Id}' already exists.  Please try your request again with a unique id or call PUT to update the existing ProjectType.");

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var validProviders = projectType.Providers
                .All(projectTypeProvider => teamCloud.Providers.Any(teamCloudProvider => teamCloudProvider.Id == projectTypeProvider.Id));

            if (!validProviders)
            {
                var validProviderIds = string.Join(", ", teamCloud.Providers.Select(p => p.Id));
                return new BadRequestObjectResult($"All provider ids on a ProjectType must match the id of a registered Provider on the TeamCloud instance.\nValid provider ids are: {validProviderIds}");
            }

            var addResult = await orchestrator
                .AddAsync(projectType)
                .ConfigureAwait(false);

            return new OkObjectResult(addResult);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProjectType projectType)
        {
            var existingProjectType = await projectTypesRepository
                .GetAsync(projectType.Id)
                .ConfigureAwait(false);

            if (existingProjectType is null)
                return new NotFoundResult();

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var validProviders = projectType.Providers
                .All(tp => teamCloud.Providers.Any(p => p.Id == tp.Id));

            if (!validProviders)
            {
                var validProviderIds = string.Join(",", teamCloud.Providers.Select(p => p.Id));
                return new BadRequestObjectResult($"All provider ids on a ProjectType must match the id of a registered Provider on the TeamCloud instance.\n Valid provider ids are: {validProviderIds}");
            }

            var addResult = await orchestrator
                .UpdateAsync(projectType)
                .ConfigureAwait(false);

            return new OkObjectResult(addResult);
        }

        [HttpDelete("{projectTypeId}")]
        public async Task<IActionResult> Delete(string projectTypeId)
        {
            var existingProjectType = await projectTypesRepository
                .GetAsync(projectTypeId)
                .ConfigureAwait(false);

            if (existingProjectType is null)
                return new NotFoundResult();

            var deleteResult = await orchestrator
                .DeleteAsync(projectTypeId)
                .ConfigureAwait(false);

            return new OkObjectResult(deleteResult);
        }
    }
}
