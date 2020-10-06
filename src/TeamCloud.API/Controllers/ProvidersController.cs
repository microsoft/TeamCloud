/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
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
    public class ProvidersController : ApiController
    {
        private readonly IProjectTypeRepository projectTypeRepository;

        public ProvidersController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IProviderRepository providerRepository, IProjectTypeRepository projectTypeRepository)
            : base(userService, orchestrator, projectRepository, providerRepository)
        {
            this.projectTypeRepository = projectTypeRepository ?? throw new ArgumentNullException(nameof(projectTypeRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetProviders", Summary = "Gets all Providers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Providers.", typeof(DataResult<List<Provider>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var providers = await ProviderRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            var returnProviders = providers.Select(p => p.PopulateExternalModel()).ToList();

            return DataResult<List<Provider>>
                .Ok(returnProviders)
                .ToActionResult();
        }


        [HttpGet("{providerId:providerId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetProviderById", Summary = "Gets a Provider by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a DataResult with the Provider as the data value.", typeof(DataResult<Provider>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the providerId provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get([FromRoute] string providerId) => EnsureProviderAsync(provider =>
        {
            return DataResult<Provider>
                .Ok(provider.PopulateExternalModel())
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.Admin)]
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
                    .ToActionResult();

            var providerDocument = await ProviderRepository
                .GetAsync(provider.Id)
                .ConfigureAwait(false);

            if (providerDocument != null)
                return ErrorResult
                    .Conflict($"A Provider with the ID '{provider.Id}' already exists on this TeamCloud Instance. Please try your request again with a unique ID or call PUT to update the existing Provider.")
                    .ToActionResult();

            if (provider.Type == ProviderType.Virtual)
            {
                var serviceProviders = await ProviderRepository
                    .ListAsync(providerType: ProviderType.Service)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var serviceProvider = serviceProviders
                    .FirstOrDefault(p => provider.Id.StartsWith($"{p.Id}.", StringComparison.Ordinal));

                if (serviceProvider is null)
                {
                    var validServiceProviderIds = string.Join(", ", serviceProviders.Select(p => p.Id));

                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "id", Message = $"No matching service provider found. Virtual provider ids must begin with the associated Service provider id followed by a period (.). Available service providers: {validServiceProviderIds}" })
                        .ToActionResult();
                }

                var urlPrefix = $"{serviceProvider.Url}?";

                if (!provider.Url.StartsWith(urlPrefix, StringComparison.OrdinalIgnoreCase))
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "url", Message = $"Virtual provider url must match the associated service provider url followed by a query string. The url should begin with {urlPrefix}" })
                        .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            providerDocument = new ProviderDocument()
                .PopulateFromExternalModel(provider);

            var command = new OrchestratorProviderCreateCommand(currentUser, providerDocument);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDocument, Provider>(command, Request)
                .ConfigureAwait(false);
        }


        [HttpPut("{providerId:providerId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProvider", Summary = "Updates an existing Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the provided Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the ID provided in the reques body was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string providerId, [FromBody] Provider provider) => EnsureProviderAsync(async providerDocument =>
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var validation = new ProviderValidator().Validate(provider);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!provider.Id.Equals(providerDocument.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"Provider's id '{provider.Id}' does match the identifier provided in the path '{providerDocument.Id}'." })
                    .ToActionResult();

            if (provider.Type == ProviderType.Virtual)
            {
                var serviceProviders = await ProviderRepository
                    .ListAsync(providerType: ProviderType.Service)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var serviceProvider = serviceProviders
                    .FirstOrDefault(p => provider.Id.StartsWith($"{p.Id}.", StringComparison.Ordinal));

                if (serviceProvider is null)
                {
                    var validServiceProviderIds = string.Join(", ", serviceProviders.Select(p => p.Id));

                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "id", Message = $"No matching service provider found. Virtual provider ids must begin with the associated Service provider id followed by a period (.). Available service providers: {validServiceProviderIds}" })
                        .ToActionResult();
                }

                var urlPrefix = $"{serviceProvider.Url}?";

                if (!provider.Url.StartsWith(urlPrefix, StringComparison.OrdinalIgnoreCase))
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "url", Message = $"Virtual provider url must match the associated service provider url followed by a query string. The url should begin with {urlPrefix}" })
                        .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            providerDocument.PopulateFromExternalModel(provider);

            var command = new OrchestratorProviderUpdateCommand(currentUser, providerDocument);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDocument, Provider>(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{providerId:providerId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "DeleteProvider", Summary = "Deletes an existing Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the provided Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string providerId) => EnsureProviderAsync(async provider =>
        {
            var projectTypes = await projectTypeRepository
                .ListByProviderAsync(provider.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            if (projectTypes.Any())
                return ErrorResult
                    .BadRequest("Cannot delete Provider because it is referenced in existing ProjectType definitions", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var projects = await ProjectRepository
                .ListByProviderAsync(provider.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            if (projects.Any())
                return ErrorResult
                    .BadRequest("Cannot delete Providers being used by existing Projects", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProviderDeleteCommand(currentUser, provider);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDocument, Provider>(command, Request)
                .ConfigureAwait(false);
        });
    }
}
