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
using TeamCloud.Adapters;
using TeamCloud.API.Auth;
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Controllers;

[ApiController]
[Route("orgs/{organizationId:organizationId}/scopes")]
[Produces("application/json")]
public class DeploymentScopesController : TeamCloudController
{
    private readonly IDeploymentScopeRepository deploymentScopeRepository;
    private readonly IComponentRepository componentRepository;
    private readonly IAdapterProvider adapterProvider;

    public DeploymentScopesController(IDeploymentScopeRepository deploymentScopeRepository,
                                      IComponentRepository componentRepository,
                                      IAdapterProvider adapterProvider,
                                      IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        this.adapterProvider = adapterProvider ?? throw new ArgumentNullException(nameof(adapterProvider));
    }


    [HttpGet]
    [Authorize(Policy = AuthPolicies.OrganizationRead)]
    [SwaggerOperation(OperationId = "GetDeploymentScopes", Summary = "Gets all Deployment Scopes.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns all Deployment Scopes.", typeof(DataResult<List<DeploymentScope>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    public async Task<IActionResult> List()
    {
        var deploymentScopes = await deploymentScopeRepository
            .ListAsync(OrganizationId)
            .ToListAsync()
            .ConfigureAwait(false);

        return DataResult<List<DeploymentScope>>
            .Ok(deploymentScopes)
            .ToActionResult();
    }


    [HttpGet("{deploymentScopeId:deploymentScopeId}")]
    [Authorize(Policy = AuthPolicies.OrganizationRead)]
    [SwaggerOperation(OperationId = "GetDeploymentScope", Summary = "Gets a Deployment Scope.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns a DeploymentScope.", typeof(DataResult<DeploymentScope>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "A DeploymentScope with the id provided was not found.", typeof(ErrorResult))]
    public Task<IActionResult> Get() => WithContextAsync<DeploymentScope>((contextUser, deploymentScope) =>
    {
        return DataResult<DeploymentScope>
            .Ok(deploymentScope)
            .ToActionResultAsync();
    });


    [HttpPost]
    [Authorize(Policy = AuthPolicies.OrganizationAdmin)]
    [Consumes("application/json")]
    [SwaggerOperation(OperationId = "CreateDeploymentScope", Summary = "Creates a new Deployment Scope.")]
    [SwaggerResponse(StatusCodes.Status201Created, "The new Deployment Scope was created.", typeof(DataResult<DeploymentScope>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "A Deployment Scope already exists with the ID provided in the request body.", typeof(ErrorResult))]
    public Task<IActionResult> Post([FromBody] DeploymentScopeDefinition deploymentScopeDefinition) => WithContextAsync<Organization>(async (contextUser, organization) =>
   {
       if (deploymentScopeDefinition is null)
           return ErrorResult
               .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
               .ToActionResult();

       if (!deploymentScopeDefinition.TryValidate(ValidatorProvider, out var definitionValidationResult))
           return ErrorResult
               .BadRequest(definitionValidationResult)
               .ToActionResult();

       var nameExists = await deploymentScopeRepository
            .NameExistsAsync(organization.Id, deploymentScopeDefinition.Slug)
            .ConfigureAwait(false);

       if (nameExists)
           return ErrorResult
               .Conflict($"A Deployment Scope with name '{deploymentScopeDefinition.DisplayName}' already exists. Deployment Scope names must be unique. Please try your request again with a unique name.")
               .ToActionResult();

       var deploymentScope = new DeploymentScope
       {
           Id = Guid.NewGuid().ToString(),
           Organization = organization.Id,
           OrganizationName = organization.Slug,
           Type = deploymentScopeDefinition.Type,
           DisplayName = deploymentScopeDefinition.DisplayName,
           InputData = deploymentScopeDefinition.InputData,
           InputDataSchema = await adapterProvider.GetAdapter(deploymentScopeDefinition.Type).GetInputDataSchemaAsync().ConfigureAwait(false),
           IsDefault = deploymentScopeDefinition.IsDefault
       };

       if (!deploymentScope.TryValidate(ValidatorProvider, out var entityValidationResult))
           return ErrorResult
               .BadRequest(entityValidationResult)
               .ToActionResult();

       var command = new DeploymentScopeCreateCommand(contextUser, deploymentScope);

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
    public Task<IActionResult> Put([FromBody] DeploymentScope deploymentScopeUpdate) => WithContextAsync<DeploymentScope>(async (contextUser, deploymentScope) =>
    {
        if (deploymentScopeUpdate is null)
            return ErrorResult
                .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                .ToActionResult();

        if (!deploymentScopeUpdate.TryValidate(ValidatorProvider, out var validationResult))
            return ErrorResult
                .BadRequest(validationResult)
                .ToActionResult();

        if (!deploymentScopeUpdate.Id.Equals(deploymentScope.Id, StringComparison.Ordinal))
            return ErrorResult
                .BadRequest(new ValidationError { Field = "id", Message = $"DeploymentScopes's id does match the identifier provided in the path." })
                .ToActionResult();

        if (deploymentScope.Type != deploymentScope.Type)
            return ErrorResult
                .BadRequest(new ValidationError { Field = "type", Message = $"DeploymentScopes's type cannot be changed." })
                .ToActionResult();

        if (deploymentScope.Type == DeploymentScopeType.AzureResourceManager)
        {
            deploymentScope = await deploymentScopeRepository.ExpandAsync(deploymentScope, true);

            if (deploymentScopeUpdate.ManagementGroupId != deploymentScope.ManagementGroupId)
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "managementGroupId", Message = $"DeploymentScopes's managementGroupId cannot be changed." })
                    .ToActionResult();

            var subsRemoved = deploymentScope.SubscriptionIds.Where(sid => !deploymentScopeUpdate.SubscriptionIds.Contains(sid));

            if (subsRemoved.Any())
            {
                var subInUse = await componentRepository
                    .ListByDeploymentScopeAsync(deploymentScope.Id)
                    .AnyAsync(c => subsRemoved.Any(s => c.ResourceId.Contains($"/subscriptions/{s}")))
                    .ConfigureAwait(false);

                if (subInUse)
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "subscriptionIds", Message = $"DeploymentScopes's subscriptionIds cannot be removed if a component is deployed in the subscription." })
                        .ToActionResult();
            }
        }

        var command = new DeploymentScopeUpdateCommand(contextUser, deploymentScope);

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
    public Task<IActionResult> Delete() => WithContextAsync<DeploymentScope>(async (contextUser, deploymentScope) =>
    {
        var command = new DeploymentScopeDeleteCommand(contextUser, deploymentScope);

        return await Orchestrator
            .InvokeAndReturnActionResultAsync(command, Request)
            .ConfigureAwait(false);
    });


}
