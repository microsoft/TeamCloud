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
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Controllers;

[ApiController]
[Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/identities")]
[Produces("application/json")]
public class ProjectIdentitiesController : TeamCloudController
{
    private readonly IProjectIdentityRepository projectIdentityRepository;

    public ProjectIdentitiesController(IProjectIdentityRepository projectIdentityRepository, IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        this.projectIdentityRepository = projectIdentityRepository ?? throw new ArgumentNullException(nameof(projectIdentityRepository));
    }


    [HttpGet]
    [Authorize(Policy = AuthPolicies.ProjectMember)]
    [SwaggerOperation(OperationId = "GetProjectIdentities", Summary = "Gets all Project Identities.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Identities.", typeof(DataResult<List<ProjectIdentity>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    public Task<IActionResult> Get() => WithContextAsync<Project>(async (contextUser, project) =>
    {
        var identities = await projectIdentityRepository
            .ListAsync(project.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        return DataResult<List<ProjectIdentity>>
            .Ok(identities)
            .ToActionResult();
    });


    [HttpGet("{projectIdentityId}")]
    [Authorize(Policy = AuthPolicies.ProjectMember)]
    [SwaggerOperation(OperationId = "GetProjectIdentity", Summary = "Gets a Project Identity.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns a ProjectIdentity.", typeof(DataResult<ProjectIdentity>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "A ProjectIdentity with the projectIdentityId provided was not found.", typeof(ErrorResult))]
    [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
    public Task<IActionResult> Get(string projectIdentityId) => WithContextAsync<ProjectIdentity>((contextUser, projectIdentity) =>
    {
        return DataResult<ProjectIdentity>
            .Ok(projectIdentity)
            .ToActionResultAsync();
    });


    [HttpPost]
    [Authorize(Policy = AuthPolicies.ProjectMember)]
    [Consumes("application/json")]
    [SwaggerOperation(OperationId = "CreateProjectIdentity", Summary = "Creates a new Project Identity.")]
    [SwaggerResponse(StatusCodes.Status201Created, "The new Project Identity was created.", typeof(DataResult<ProjectIdentity>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Identity already exists with the ID provided in the request body.", typeof(ErrorResult))]
    public Task<IActionResult> Post([FromBody] ProjectIdentityDefinition projectIdentityDefinition) => WithContextAsync<Project>(async (contextUser, project) =>
    {
        if (projectIdentityDefinition is null)
            return ErrorResult
                .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                .ToActionResult();

        var projectIdentity = new ProjectIdentity
        {
            Id = Guid.NewGuid().ToString(),
            Organization = project.Organization,
            OrganizationName = project.OrganizationName,
            ProjectId = project.Id,
            ProjectName = project.Slug,
            DisplayName = projectIdentityDefinition.DisplayName,
            DeploymentScopeId = projectIdentityDefinition.DeploymentScopeId
        };

        var command = new ProjectIdentityCreateCommand(contextUser, projectIdentity);

        return await Orchestrator
            .InvokeAndReturnActionResultAsync(command, Request)
            .ConfigureAwait(false);
    });


    [HttpPut("{projectIdentityId}")]
    [Authorize(Policy = AuthPolicies.ProjectMember)]
    [Consumes("application/json")]
    [SwaggerOperation(OperationId = "UpdateProjectIdentity", Summary = "Updates an existing Project Identity.")]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project Identity. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Identity with the key provided in the request body was not found.", typeof(ErrorResult))]
    public Task<IActionResult> Put([FromRoute] string projectIdentityId, [FromBody] ProjectIdentity projectIdentityUpdate) => WithContextAsync<ProjectIdentity>(async (contextUser, projectIdentity) =>
    {
        if (projectIdentityUpdate is null)
            throw new ArgumentNullException(nameof(projectIdentityUpdate));

        var validation = await projectIdentityUpdate
            .ValidateAsync(ValidatorProvider)
            .ConfigureAwait(false);

        if (!validation.IsValid)
            return ErrorResult
                .BadRequest(validation)
                .ToActionResult();

        if (!projectIdentityUpdate.Id.Equals(projectIdentityId, StringComparison.Ordinal))
            return ErrorResult
                .BadRequest(new ValidationError { Field = "id", Message = $"Project Identity's id does match the identifier provided in the path." })
                .ToActionResult();

        projectIdentity.RedirectUrls = projectIdentityUpdate.RedirectUrls;

        var command = new ProjectIdentityUpdateCommand(contextUser, projectIdentity);

        return await Orchestrator
            .InvokeAndReturnActionResultAsync(command, Request)
            .ConfigureAwait(false);
    });


    [HttpDelete("{projectIdentityId}")]
    [Authorize(Policy = AuthPolicies.ProjectMember)]
    [SwaggerOperation(OperationId = "DeleteProjectIdentity", Summary = "Deletes a Project Identity.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The ProjectIdentity was deleted.", typeof(DataResult<ProjectIdentity>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "A ProjectIdentity with the projectIdentityId provided was not found.", typeof(ErrorResult))]
    [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
    public Task<IActionResult> Delete([FromRoute] string projectIdentityId) => WithContextAsync<ProjectIdentity>(async (contextUser, projectIdentity) =>
    {
        var command = new ProjectIdentityDeleteCommand(contextUser, projectIdentity);

        return await Orchestrator
            .InvokeAndReturnActionResultAsync(command, Request)
            .ConfigureAwait(false);
    });
}
