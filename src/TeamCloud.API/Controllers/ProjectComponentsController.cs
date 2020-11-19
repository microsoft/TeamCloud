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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using ValidationError = TeamCloud.API.Data.Results.ValidationError;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{org}/projects/{projectId:projectId}/components")]
    [Produces("application/json")]
    public class ProjectComponentsController : ApiController
    {
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;

        public ProjectComponentsController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IComponentRepository componentRepository, IComponentTemplateRepository componentTemplateRepository)
            : base(userService, orchestrator, organizationRepository, projectRepository)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectComponents", Summary = "Gets all Components for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Components", typeof(DataResult<List<Component>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectIdAsync(async projectId =>
        {
            var components = await componentRepository
                .ListAsync(OrgId, projectId)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<Component>>
                .Ok(components)
                .ToActionResult();
        });


        [HttpGet("{id}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectComponent", Summary = "Gets a Project Component.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project Component", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Component with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string id) => EnsureProjectIdAsync(async projectId =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var component = await componentRepository
                .GetAsync(OrgId, id)
                .ConfigureAwait(false);

            if (component is null || !component.ProjectId.Equals(projectId, StringComparison.Ordinal))
                return ErrorResult
                    .NotFound($"A Component with the ID '{id}' could not be found for Project {projectId}.")
                    .ToActionResult();

            return DataResult<Component>
                .Ok(component)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectComponentWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectComponent", Summary = "Creates a new Project Component.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Component.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Component already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ComponentRequest request) => EnsureProjectAndProjectTemplateAsync(async (project, projectTemplate) =>
        {
            if (request is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            // if (!component.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
            //     return ErrorResult
            //         .BadRequest(validationResult)
            //         .ToActionResult();

            var componentTemplate = await componentTemplateRepository
                .GetAsync(OrgId, request.TemplateId)
                .ConfigureAwait(false);

            if (componentTemplate is null || !componentTemplate.ParentId.Equals(projectTemplate.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .NotFound($"A ComponentTemplate with the id '{request.TemplateId}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            var input = JObject.Parse(request.InputJson);
            var schema = JSchema.Parse(componentTemplate.InputJsonSchema);

            if (!input.IsValid(schema, out IList<string> schemaErrors))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "input", Message = $"ComponentRequest's input does not match the the Component Templates inputJsonSchema.  Errors: {string.Join(", ", schemaErrors)}." })
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync(OrgId)
                .ConfigureAwait(false);

            var component = new Component
            {
                TemplateId = componentTemplate.Id,
                Organization = project.Organization,
                ProjectId = project.Id,
                Provider = componentTemplate.Provider,
                RequestedBy = currentUser.Id,
                DisplayName = componentTemplate.DisplayName,
                InputJson = request.InputJson,
                Type = componentTemplate.Type
            };

            var command = new ProjectComponentCreateCommand(currentUser, component, project.Id);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{id}")]
        [Authorize(Policy = AuthPolicies.ProjectComponentUpdate)]
        [SwaggerOperation(OperationId = "DeleteProjectComponent", Summary = "Deletes an existing Project Component.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The Project Component was deleted.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided id was not found, or a Component with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string id) => EnsureProjectIdAsync(async projectId =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var component = await componentRepository
                .GetAsync(OrgId, id)
                .ConfigureAwait(false);

            if (component is null || component.ProjectId.Equals(projectId, StringComparison.Ordinal))
                return ErrorResult
                    .NotFound($"A Component with the id '{id}' could not be found for Project {projectId}.")
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync(OrgId)
                .ConfigureAwait(false);

            var command = new ProjectComponentDeleteCommand(currentUser, component, projectId);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}

