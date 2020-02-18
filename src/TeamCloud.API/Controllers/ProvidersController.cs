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
using TeamCloud.API.Data;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/providers")]
    [Produces("application/json")]
    public class ProvidersController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IProjectsRepositoryReadOnly projectsRepository;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;
        readonly IProjectTypesRepositoryReadOnly projectTypesRepository;

        public ProvidersController(UserService userService, Orchestrator orchestrator, IProjectsRepositoryReadOnly projectsRepository, ITeamCloudRepositoryReadOnly teamCloudRepository, IProjectTypesRepositoryReadOnly projectTypesRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };


        [HttpGet]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetProviders", Summary = "Gets all Providers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Providers.", typeof(DataResult<List<Provider>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var providers = teamCloudInstance?.Providers ?? new List<Provider>();

            return DataResult<List<Provider>>
                .Ok(providers.ToList())
                .ActionResult();

        }


        [HttpGet("{providerId}")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetProviderById", Summary = "Gets a Provider by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a DataResult with the Provider as the data value.", typeof(DataResult<Provider>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Provider with the providerId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get(string providerId)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var provider = teamCloudInstance.Providers?.FirstOrDefault(p => p.Id == providerId);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{providerId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            return DataResult<Provider>
                .Ok(provider)
                .ActionResult();
        }


        [HttpPost]
        [Authorize(Policy = "admin")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProvider", Summary = "Creates a new Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The Provider provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Provider already exists with the ID provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] Provider provider)
        {
            var validation = new ProviderValidator().Validate(provider);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            if (teamCloudInstance.Providers.Contains(provider))
                return ErrorResult
                    .Conflict($"A Provider with the ID '{provider.Id}' already exists on this TeamCloud Instance. Please try your request again with a unique ID or call PUT to update the existing Provider.")
                    .ActionResult();

            var command = new ProviderCreateCommand(CurrentUser, provider);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }


        [HttpPut]
        [Authorize(Policy = "admin")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProvider", Summary = "Updates an existing Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the provided Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The Provider provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Provider with the ID provided in the reques body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] Provider provider)
        {
            var validation = new ProviderValidator().Validate(provider);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var oldProvider = teamCloudInstance.Providers?.FirstOrDefault(p => p.Id == provider.Id);

            if (oldProvider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{provider.Id}' could not be found on this TeamCloud Instance.")
                    .ActionResult();

            var command = new ProviderUpdateCommand(CurrentUser, provider);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }


        [HttpDelete("{providerId}")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "DeleteProvider", Summary = "Deletes an existing Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the provided Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Provider with the provided providerId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete(string providerId)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var provider = teamCloudInstance.Providers?.FirstOrDefault(p => p.Id == providerId);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{providerId}' could not be found on this TeamCloud Instance.")
                    .ActionResult();

            // TODO: Query via the database query instead of getting all
            var projectTypes = await projectTypesRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            if (projectTypes.Any(pt => pt.Providers.Any(pr => pr.Id == providerId)))
                return ErrorResult
                    .BadRequest("Cannot delete Providers referenced in existing ProjectType definitions", ResultErrorCodes.ValidationError)
                    .ActionResult();

            // TODO: Query via the database query instead of getting all
            var projects = await projectsRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            if (projects.Any(p => p.Type.Providers.Any(pr => pr.Id == providerId)))
                if (projectTypes.Any(pt => pt.Providers.Any(pr => pr.Id == providerId)))
                    return ErrorResult
                        .BadRequest("Cannot delete Providers being used by existing Projects", ResultErrorCodes.ValidationError)
                        .ActionResult();

            var command = new ProviderDeleteCommand(CurrentUser, provider);

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
