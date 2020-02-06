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
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

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
        public async Task<IActionResult> Get()
        {
            var projectTypes = await projectTypesRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<ProjectType>>
                .Ok(projectTypes)
                .ActionResult();
        }


        [HttpGet("{projectTypeId}")]
        public async Task<IActionResult> Get(string projectTypeId)
        {
            var projectType = await projectTypesRepository
                .GetAsync(projectTypeId)
                .ConfigureAwait(false);

            if (projectType is null)
                return ErrorResult
                    .NotFound($"A ProjectType with the ID '{projectTypeId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            return DataResult<ProjectType>
                .Ok(projectType)
                .ActionResult();
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProjectType projectType)
        {
            var validation = new ProjectTypeValidator().Validate(projectType);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var existingProjectType = await projectTypesRepository
                .GetAsync(projectType.Id)
                .ConfigureAwait(false);

            if (existingProjectType != null)
                return ErrorResult
                    .Conflict($"A ProjectType with id '{projectType.Id}' already exists.  Please try your request again with a unique id or call PUT to update the existing ProjectType.")
                    .ActionResult();

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var validProviders = projectType.Providers
                .All(projectTypeProvider => teamCloud.Providers.Any(teamCloudProvider => teamCloudProvider.Id == projectTypeProvider.Id));

            if (!validProviders)
            {
                var validProviderIds = string.Join(", ", teamCloud.Providers.Select(p => p.Id));
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectType", Message = $"All provider ids on a ProjectType must match the id of a registered Provider on the TeamCloud instance. Valid provider ids are: {validProviderIds}" })
                    .ActionResult();
            }

            var addResult = await orchestrator
                .AddAsync(projectType)
                .ConfigureAwait(false);

            var baseUrl = HttpContext.GetApplicationBaseUrl();
            var location = new Uri(baseUrl, $"api/projectTypes/{addResult.Id}").ToString();

            return DataResult<ProjectType>
                .Created(addResult, location)
                .ActionResult();
        }


        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProjectType projectType)
        {
            var validation = new ProjectTypeValidator().Validate(projectType);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var existingProjectType = await projectTypesRepository
                .GetAsync(projectType.Id)
                .ConfigureAwait(false);

            if (existingProjectType is null)
                return ErrorResult
                    .NotFound($"A ProjectType with the ID '{projectType.Id}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            var teamCloud = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var validProviders = projectType.Providers
                .All(tp => teamCloud.Providers.Any(p => p.Id == tp.Id));

            if (!validProviders)
            {
                var validProviderIds = string.Join(",", teamCloud.Providers.Select(p => p.Id));
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectType", Message = $"All provider ids on a ProjectType must match the id of a registered Provider on the TeamCloud instance. Valid provider ids are: {validProviderIds}" })
                    .ActionResult();
            }

            var updateResult = await orchestrator
                .UpdateAsync(projectType)
                .ConfigureAwait(false);

            return DataResult<ProjectType>
                .Ok(updateResult)
                .ActionResult();
        }


        [HttpDelete("{projectTypeId}")]
        public async Task<IActionResult> Delete(string projectTypeId)
        {
            var existingProjectType = await projectTypesRepository
                .GetAsync(projectTypeId)
                .ConfigureAwait(false);

            if (existingProjectType is null)
                return ErrorResult
                    .NotFound($"A ProjectType with the ID '{projectTypeId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            _ = await orchestrator
                .DeleteAsync(projectTypeId)
                .ConfigureAwait(false);

            return DataResult<ProjectType>
                .NoContent()
                .ActionResult();
        }
    }
}
