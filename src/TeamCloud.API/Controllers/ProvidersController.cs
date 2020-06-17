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
using TeamCloud.API.Data.Results;
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
        readonly IProjectsRepository projectsRepository;
        readonly ITeamCloudRepository teamCloudRepository;
        readonly IProvidersRepository providersRepository;
        readonly IProjectTypesRepository projectTypesRepository;

        public ProvidersController(UserService userService, Orchestrator orchestrator, IProjectsRepository projectsRepository, ITeamCloudRepository teamCloudRepository, IProvidersRepository providersRepository, IProjectTypesRepository projectTypesRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }


        [HttpGet]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetProviders", Summary = "Gets all Providers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Providers.", typeof(DataResult<List<Provider>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var providers = await providersRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<Provider>>
                .Ok(providers)
                .ActionResult();
        }


        [HttpGet("{providerId}")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetProviderById", Summary = "Gets a Provider by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a DataResult with the Provider as the data value.", typeof(DataResult<Provider>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the providerId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get(string providerId)
        {
            var provider = await providersRepository
                .GetAsync(providerId)
                .ConfigureAwait(false);

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
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Provider already exists with the ID provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] Provider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var validation = new ProviderValidator().Validate(provider);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var existingProvider = await providersRepository
                .GetAsync(provider.Id)
                .ConfigureAwait(false);

            if (existingProvider != null)
                return ErrorResult
                    .Conflict($"A Provider with the ID '{provider.Id}' already exists on this TeamCloud Instance. Please try your request again with a unique ID or call PUT to update the existing Provider.")
                    .ActionResult();

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProviderCreateCommand(currentUserForCommand, provider);

            return await orchestrator
                .InvokeAndReturnAccepted(command)
                .ConfigureAwait(false);
        }


        [HttpPut]
        [Authorize(Policy = "admin")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProvider", Summary = "Updates an existing Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the provided Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the ID provided in the reques body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] Provider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var validation = new ProviderValidator().Validate(provider);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var oldProvider = await providersRepository
                .GetAsync(provider.Id)
                .ConfigureAwait(false);

            if (oldProvider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{provider.Id}' could not be found on this TeamCloud Instance.")
                    .ActionResult();

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProviderUpdateCommand(currentUserForCommand, provider);

            return await orchestrator
                .InvokeAndReturnAccepted(command)
                .ConfigureAwait(false);
        }


        [HttpDelete("{providerId}")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "DeleteProvider", Summary = "Deletes an existing Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the provided Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete(string providerId)
        {
            var provider = await providersRepository
                .GetAsync(providerId)
                .ConfigureAwait(false);

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
                    .BadRequest("Cannot delete Providers referenced in existing ProjectType definitions", ResultErrorCode.ValidationError)
                    .ActionResult();

            // TODO: Query via the database query instead of getting all
            var projects = await projectsRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            if (projects.Any(p => p.Type.Providers.Any(pr => pr.Id == providerId)))
                if (projectTypes.Any(pt => pt.Providers.Any(pr => pr.Id == providerId)))
                    return ErrorResult
                        .BadRequest("Cannot delete Providers being used by existing Projects", ResultErrorCode.ValidationError)
                        .ActionResult();

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProviderDeleteCommand(currentUserForCommand, provider);

            return await orchestrator
                .InvokeAndReturnAccepted(command)
                .ConfigureAwait(false);
        }
    }
}
