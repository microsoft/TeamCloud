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
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{org}/scopes")]
    [Produces("application/json")]
    public class DeploymentScopesController : ApiController
    {
        private readonly IDeploymentScopeRepository deploymentScopeRepository;

        public DeploymentScopesController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IDeploymentScopeRepository deploymentScopeRepository)
            : base(userService, orchestrator, organizationRepository)
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetDeploymentScopes", Summary = "Gets all Deployment Scopes.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Deployment Scopes.", typeof(DataResult<List<DeploymentScope>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ResolveOrganizationIdAsync(async organizationId =>
        {
            var deploymentScopes = await deploymentScopeRepository
                .ListAsync(organizationId)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<DeploymentScope>>
                .Ok(deploymentScopes)
                .ToActionResult();
        });


        [HttpGet("{id}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetDeploymentScopeById", Summary = "Gets a Deployment Scope by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a DeploymentScope.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A DeploymentScope with the id provided was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get(string id) => ResolveOrganizationIdAsync(async organizationId =>
        {
            var deploymentScope = await deploymentScopeRepository
                .GetAsync(organizationId, id)
                .ConfigureAwait(false);

            if (deploymentScope is null)
                return ErrorResult
                    .NotFound($"A Deployemnt Scope with the ID '{id}' could not be found in this Organization")
                    .ToActionResult();

            return DataResult<DeploymentScope>
                .Ok(deploymentScope)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectTemplate", Summary = "Creates a new Deployment Scope.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new Deployment Scope was created.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Deployment Scope already exists with the ID provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] DeploymentScopeDefinition deploymentScopeDefinition) => ResolveOrganizationIdAsync(async organizationId =>
        {
            if (deploymentScopeDefinition is null)
                return ErrorResult
                    .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            // if (!projectTemplate.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
            //     return ErrorResult
            //         .BadRequest(validationResult)
            //         .ToActionResult();

            // var projectTemplate = await projectTemplateRepository
            //     .GetAsync(organizationId, projectTemplateDefinition.Id)
            //     .ConfigureAwait(false);

            //     .ConfigureAwait(false);

            //  // if (projectTemplate != null)
            //         .ToActionResult();

            // var providers = await ProviderRepository
            //     .ListAsync(includeServiceProviders: false)
            //     .ToListAsync()
            //     .ConfigureAwait(false);

            // var validProviders = projectTemplate.Providers
            //     .All(p => providers.Any(provider => provider.Id == p.Id));

            // if (!validProviders)
            // {
            //     var validProviderIds = string.Join(", ", providers.Select(p => p.Id));

            //     return ErrorResult
            //         .BadRequest(new ValidationError { Field = "projectTemplate", Message = $"All provider ids on a ProjectTemplate must match the id of a registered Provider on the TeamCloud instance and cannot be a Service Provider. Valid provider ids are: {validProviderIds}" })
            //         .ToActionResult();
            // }

            var currentUser = await UserService
                .CurrentUserAsync(organizationId)
                .ConfigureAwait(false);

            var deploymentScope = new DeploymentScope
            {
                Id = Guid.NewGuid().ToString(),
                Organization = organizationId,
                DisplayName = deploymentScopeDefinition.DisplayName,
                ManagementGroupId = deploymentScopeDefinition.ManagementGroupId,
                IsDefault = deploymentScopeDefinition.IsDefault
            };

            var command = new OrchestratorDeploymentScopeCreateCommand(currentUser, deploymentScope);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{id}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateDeploymentScope", Summary = "Updates an existing Deployment Scope.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The DeploymentScope was updated.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Deployment Scope with the ID provided in the request body could not be found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string id, [FromBody] DeploymentScope deploymentScope) => ResolveOrganizationIdAsync(async organizationId =>
        {
            if (deploymentScope is null)
                return ErrorResult
                    .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!deploymentScope.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            var existingDeploymentScope = await deploymentScopeRepository
                .GetAsync(organizationId, id)
                .ConfigureAwait(false);

            if (!deploymentScope.Id.Equals(existingDeploymentScope.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"DeploymentScopes's id does match the identifier provided in the path." })
                    .ToActionResult();

            // var providers = await ProviderRepository
            //     .ListAsync(includeServiceProviders: false)
            //     .ToListAsync()
            //     .ConfigureAwait(false);

            // var validProviders = projectTemplate.Providers
            //     .All(p => providers.Any(provider => provider.Id == p.Id));

            // if (!validProviders)
            // {
            //     var validProviderIds = string.Join(", ", providers.Select(p => p.Id));

            //     return ErrorResult
            //         .BadRequest(new ValidationError { Field = "projectTemplate", Message = $"All provider ids on a ProjectTemplate must match the id of a registered Provider on the TeamCloud instance and cannot be a Service Provider. Valid provider ids are: {validProviderIds}" })
            //         .ToActionResult();
            // }

            var currentUser = await UserService
                .CurrentUserAsync(OrganizationId)
                .ConfigureAwait(false);

            var command = new OrchestratorDeploymentScopeUpdateCommand(currentUser, deploymentScope);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{id}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "DeleteDeploymentScope", Summary = "Deletes a Deployment Scope.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The DeploymentScope was deleted.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A DeploymentScope with the id provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string id) => ResolveOrganizationIdAsync(async organizationId =>
        {
            var deploymentScope = await deploymentScopeRepository
                .GetAsync(organizationId, id)
                .ConfigureAwait(false);

            var currentUser = await UserService
                .CurrentUserAsync(OrganizationId)
                .ConfigureAwait(false);

            var command = new OrchestratorDeploymentScopeDeleteCommand(currentUser, deploymentScope);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}