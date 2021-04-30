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
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;
using ValidationError = TeamCloud.API.Data.Results.ValidationError;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/components")]
    [Produces("application/json")]
    public partial class ComponentsController : TeamCloudController
    {
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IProjectTemplateRepository projectTemplateRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;

        public ComponentsController(IComponentRepository componentRepository, IComponentTemplateRepository componentTemplateRepository, IProjectTemplateRepository projectTemplateRepository, IDeploymentScopeRepository deploymentScopeRepository) : base()
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.projectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetComponents", Summary = "Gets all Components for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Components", typeof(DataResult<List<Component>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromQuery] bool deleted = false) => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            var components = await componentRepository
                .ListAsync(context.Project.Id, deleted)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<Component>>
                .Ok(components)
                .ToActionResult();
        });


        [HttpGet("{id}")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetComponent", Summary = "Gets a Project Component.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project Component", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Component with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string id) => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var component = await componentRepository
                .GetAsync(context.Project.Id, id, true)
                .ConfigureAwait(false);

            if (component is null)
                return ErrorResult
                    .NotFound($"A Component with the ID '{id}' could not be found for Project {context.Project.Id}.")
                    .ToActionResult();

            return DataResult<Component>
                .Ok(component)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateComponent", Summary = "Creates a new Project Component.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Component.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Component already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ComponentDefinition componentDefinition) => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            if (componentDefinition is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!componentDefinition.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            var projectTemplate = await projectTemplateRepository
                .GetAsync(context.Project.Organization, context.Project.Template)
                .ConfigureAwait(false);

            var componentTemplate = await componentTemplateRepository
                .GetAsync(context.Organization.Id, context.Project.Id, componentDefinition.TemplateId)
                .ConfigureAwait(false);

            if (componentTemplate is null || !componentTemplate.ParentId.Equals(projectTemplate.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .NotFound($"A ComponentTemplate with the id '{componentDefinition.TemplateId}' could not be found for Project {context.Project.Id}.")
                    .ToActionResult();

            if (!string.IsNullOrWhiteSpace(componentDefinition.InputJson))
            {
                var input = JObject.Parse(componentDefinition.InputJson);
                var schema = JSchema.Parse(componentTemplate.InputJsonSchema);

                if (!input.IsValid(schema, out IList<string> schemaErrors))
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "input", Message = $"ComponentDefinition's input does not match the the Component Templates inputJsonSchema.  Errors: {string.Join(", ", schemaErrors)}." })
                        .ToActionResult();
            }

            if (Guid.TryParse(componentDefinition.DeploymentScopeId, out Guid deploymentScopeId))
            {
                var deploymentScope = await deploymentScopeRepository
                    .GetAsync(context.Organization.Id, deploymentScopeId.ToString())
                    .ConfigureAwait(false);

                if (deploymentScope is null)
                    return ErrorResult
                        .NotFound($"A DeploymentScope with the id '{deploymentScopeId}' could not be found for Project {context.Project.Id}.")
                        .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync(context.Organization.Id)
                .ConfigureAwait(false);

            var component = new Component
            {
                TemplateId = componentTemplate.Id,
                DeploymentScopeId = componentDefinition.DeploymentScopeId,
                Organization = context.Project.Organization,
                ProjectId = context.Project.Id,
                Creator = currentUser.Id,
                DisplayName = componentDefinition.DisplayName,
                InputJson = componentDefinition.InputJson,
                Type = componentTemplate.Type
            };

            var command = new ComponentCreateCommand(currentUser, component);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{id}")]
        [Authorize(Policy = AuthPolicies.ProjectComponentOwner)]
        [SwaggerOperation(OperationId = "DeleteComponent", Summary = "Deletes an existing Project Component.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The Project Component was deleted.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided id was not found, or a Component with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string id) => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var component = await componentRepository
                .GetAsync(context.Project.Id, id)
                .ConfigureAwait(false);

            if (component is null || !component.ProjectId.Equals(context.Project.Id, StringComparison.Ordinal))
                return ErrorResult
                    .NotFound($"A Component with the id '{id}' could not be found for Project {context.Project.Id}.")
                    .ToActionResult();

            if (component.Deleted.HasValue)
                return ErrorResult
                    .BadRequest($"The component has already been (soft) deleted and is pending final deletion.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var command = new ComponentDeleteCommand(context.ContextUser, component);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}

