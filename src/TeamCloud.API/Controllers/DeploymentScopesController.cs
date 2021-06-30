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
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/scopes")]
    [Produces("application/json")]
    public class DeploymentScopesController : TeamCloudController
    {
        private readonly IDeploymentScopeRepository deploymentScopeRepository;

        public DeploymentScopesController(IDeploymentScopeRepository deploymentScopeRepository) : base()
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetDeploymentScopes", Summary = "Gets all Deployment Scopes.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Deployment Scopes.", typeof(DataResult<List<DeploymentScope>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
        {
            var deploymentScopes = await deploymentScopeRepository
                .ListAsync(context.Organization.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<DeploymentScope>>
                .Ok(deploymentScopes)
                .ToActionResult();
        });


        [HttpGet("{deploymentScopeId:deploymentScopeId}")]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetDeploymentScope", Summary = "Gets a Deployment Scope.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a DeploymentScope.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A DeploymentScope with the id provided was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get(string deploymentScopeId) => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
        {
            var deploymentScope = await deploymentScopeRepository
                .GetAsync(context.Organization.Id, deploymentScopeId)
                .ConfigureAwait(false);

            if (deploymentScope is null)
                return ErrorResult
                    .NotFound($"A Deployemnt Scope with the ID '{deploymentScopeId}' could not be found in this Organization")
                    .ToActionResult();

            return DataResult<DeploymentScope>
                .Ok(deploymentScope)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.OrganizationAdmin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateDeploymentScope", Summary = "Creates a new Deployment Scope.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new Deployment Scope was created.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Deployment Scope already exists with the ID provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] DeploymentScopeDefinition deploymentScopeDefinition) => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
       {
           if (deploymentScopeDefinition is null)
               return ErrorResult
                   .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                   .ToActionResult();

           if (!deploymentScopeDefinition.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
               return ErrorResult
                   .BadRequest(validationResult)
                   .ToActionResult();

           var deploymentScope = new DeploymentScope
           {
               Id = Guid.NewGuid().ToString(),
               Organization = context.Organization.Id,
               Type = deploymentScopeDefinition.Type,
               DisplayName = deploymentScopeDefinition.DisplayName,
               InputData = deploymentScopeDefinition.InputData,
               IsDefault = deploymentScopeDefinition.IsDefault
           };

           var command = new DeploymentScopeCreateCommand(context.ContextUser, deploymentScope);

           return await Orchestrator
               .InvokeAndReturnActionResultAsync(command, Request)
               .ConfigureAwait(false);
       });


        [HttpPut("{deploymentScopeId:deploymentScopeId}")]
        [Authorize(Policy = AuthPolicies.OrganizationAdmin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateDeploymentScope", Summary = "Updates an existing Deployment Scope.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The DeploymentScope was updated.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Deployment Scope with the ID provided in the request body could not be found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string deploymentScopeId, [FromBody] DeploymentScope deploymentScope) => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
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
                .GetAsync(context.Organization.Id, deploymentScopeId)
                .ConfigureAwait(false);

            if (!deploymentScope.Id.Equals(existingDeploymentScope.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"DeploymentScopes's id does match the identifier provided in the path." })
                    .ToActionResult();

            var command = new DeploymentScopeUpdateCommand(context.ContextUser, deploymentScope);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{deploymentScopeId:deploymentScopeId}")]
        [Authorize(Policy = AuthPolicies.OrganizationAdmin)]
        [SwaggerOperation(OperationId = "DeleteDeploymentScope", Summary = "Deletes a Deployment Scope.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The DeploymentScope was deleted.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A DeploymentScope with the id provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string deploymentScopeId) => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
        {
            var deploymentScope = await deploymentScopeRepository
                .GetAsync(context.Organization.Id, deploymentScopeId)
                .ConfigureAwait(false);

            if (deploymentScope is null)
                return ErrorResult
                    .NotFound($"A Deployemnt Scope with the ID '{deploymentScopeId}' could not be found in this Organization")
                    .ToActionResult();

            var command = new DeploymentScopeDeleteCommand(context.ContextUser, deploymentScope);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });

        [HttpPut("{deploymentScopeId:deploymentScopeId}/authorize")]
        [Authorize(Policy = AuthPolicies.OrganizationAdmin)]
        [SwaggerOperation(OperationId = "AuthorizeDeploymentScope", Summary = "Authorize an existing Deployment Scope.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The DeploymentScope was updated.", typeof(DataResult<DeploymentScope>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A DeploymentScope with the id provided was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Authorize([FromRoute] string deploymentScopeId, [FromBody] DeploymentScope deploymentScope) => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
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
                .GetAsync(context.Organization.Id, deploymentScopeId)
                .ConfigureAwait(false);

            if (!deploymentScope.Id.Equals(existingDeploymentScope.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"DeploymentScopes's id does match the identifier provided in the path." })
                    .ToActionResult();

            var command = new DeploymentScopeAuthorizeCommand(context.ContextUser, existingDeploymentScope);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}
