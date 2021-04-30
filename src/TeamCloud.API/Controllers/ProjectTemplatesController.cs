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
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Git.Services;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/templates")]
    [Produces("application/json")]
    public class ProjectTemplatesController : TeamCloudController
    {
        private readonly IProjectTemplateRepository projectTemplateRepository;
        private readonly IRepositoryService repositoryService;

        public ProjectTemplatesController(IProjectTemplateRepository projectTemplateRepository, IRepositoryService repositoryService) : base()
        {
            this.projectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
            this.repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetProjectTemplates", Summary = "Gets all Project Templates.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Templates.", typeof(DataResult<List<ProjectTemplate>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
        {
            var projectTemplates = await projectTemplateRepository
                .ListAsync(context.Organization.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var freshTemplates = await Task
                .WhenAll(projectTemplates.Select(pt => repositoryService.UpdateProjectTemplateAsync(pt)))
                .ConfigureAwait(false);

            return DataResult<List<ProjectTemplate>>
                .Ok(freshTemplates?.ToList() ?? projectTemplates)
                .ToActionResult();
        });


        [HttpGet("{projectTemplateId}")]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetProjectTemplate", Summary = "Gets a Project Template.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a ProjectTemplate.", typeof(DataResult<ProjectTemplate>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProjectTemplate with the projectTemplateId provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get(string projectTemplateId) => ExecuteAsync<TeamCloudProjectTemplateContext>(async context =>
        {
            var projectTemplate = await repositoryService
                .UpdateProjectTemplateAsync(context.ProjectTemplate)
                .ConfigureAwait(false);

            return DataResult<ProjectTemplate>
                .Ok(projectTemplate)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.OrganizationAdmin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectTemplate", Summary = "Creates a new Project Template.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new Project Template was created.", typeof(DataResult<ProjectTemplate>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Template already exists with the ID provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ProjectTemplateDefinition projectTemplateDefinition) => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
        {
            if (projectTemplateDefinition is null)
                return ErrorResult
                    .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync(context.Organization.Id)
                .ConfigureAwait(false);

            var projectTemplate = new ProjectTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Organization = context.Organization.Id,
                DisplayName = projectTemplateDefinition.DisplayName,
                Repository = new RepositoryReference
                {
                    Url = projectTemplateDefinition.Repository.Url,
                    Token = projectTemplateDefinition.Repository.Token,
                    Version = projectTemplateDefinition.Repository.Version
                }
            };

            var command = new ProjectTemplateCreateCommand(currentUser, projectTemplate);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{projectTemplateId}")]
        [Authorize(Policy = AuthPolicies.OrganizationAdmin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectTemplate", Summary = "Updates an existing Project Template.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The ProjectTemplate was updated.", typeof(DataResult<ProjectTemplate>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Template with the ID provided in the request body could not be found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string projectTemplateId, [FromBody] ProjectTemplate updatedProjectTemplate) => ExecuteAsync<TeamCloudProjectTemplateContext>(async context =>
        {
            if (updatedProjectTemplate is null)
                return ErrorResult
                    .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!updatedProjectTemplate.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            if (!context.ProjectTemplate.Id.Equals(updatedProjectTemplate.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"ProjectTemplate's id does match the identifier provided in the path." })
                    .ToActionResult();

            var command = new ProjectTemplateUpdateCommand(context.ContextUser, updatedProjectTemplate);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{projectTemplateId}")]
        [Authorize(Policy = AuthPolicies.OrganizationAdmin)]
        [SwaggerOperation(OperationId = "DeleteProjectTemplate", Summary = "Deletes a Project Template.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The ProjectTemplate was deleted.", typeof(DataResult<ProjectTemplate>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProjectTemplate with the projectTemplateId provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string projectTemplateId) => ExecuteAsync<TeamCloudProjectTemplateContext>(async context =>
        {
            var command = new ProjectTemplateDeleteCommand(context.ContextUser, context.ProjectTemplate);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}
