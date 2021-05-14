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
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;
using ValidationError = TeamCloud.API.Data.Results.ValidationError;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/components/{componentId:componentId}/tasks")]
    [Produces("application/json")]
    public class ComponentTasksController : TeamCloudController
    {
        private readonly IComponentTaskRepository componentTaskRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;

        public ComponentTasksController(IComponentTaskRepository componentTaskRepository, IComponentTemplateRepository componentTemplateRepository) : base()
        {
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetComponentTasks", Summary = "Gets all Component Tasks.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Component Tasks", typeof(DataResult<List<ComponentTask>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Component with the provided componentId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ExecuteAsync<TeamCloudProjectComponentContext>(async context =>
       {
           var componenetTasks = await componentTaskRepository
               .ListAsync(context.Component.Id)
               .ToListAsync()
               .ConfigureAwait(false);

           return DataResult<List<ComponentTask>>
               .Ok(componenetTasks)
               .ToActionResult();
       });


        [HttpGet("{id}")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetComponentTask", Summary = "Gets the Component Task.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a Component Task", typeof(DataResult<ComponentTask>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Component Task with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string id) => ExecuteAsync<TeamCloudProjectComponentContext>(async context =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var componentTask = await componentTaskRepository
                .GetAsync(context.Component.Id, id, true)
                .ConfigureAwait(false);

            if (componentTask is null)
                return ErrorResult
                    .NotFound($"A Component Task with the id '{id}' could not be found for Component {context.Component.Id}.")
                    .ToActionResult();

            return DataResult<ComponentTask>
                .Ok(componentTask)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [Consumes("application/json")] // TODO: should this be only allowed by AuthPolicies.ProjectComponentOwner
        [SwaggerOperation(OperationId = "CreateComponentTask", Summary = "Creates a new Project Component Task.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Component Task.", typeof(DataResult<ComponentTask>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Component already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ComponentTaskDefinition componentTaskDefinition) => ExecuteAsync<TeamCloudProjectComponentContext>(async context =>
        {
            if (componentTaskDefinition is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!componentTaskDefinition.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            var componentTemplate = await componentTemplateRepository
                .GetAsync(context.Organization.Id, context.Project.Id, context.Component.TemplateId)
                .ConfigureAwait(false);

            if (componentTemplate is null || !componentTemplate.ParentId.Equals(context.Project.Template, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .NotFound($"A ComponentTemplate with the id '{context.Component.TemplateId}' could not be found for Project {context.Project.Id}.")
                    .ToActionResult();

            var componentTaskTemplate = componentTemplate.Tasks.FirstOrDefault(t => t.Id.Equals(componentTaskDefinition.TaskId, StringComparison.OrdinalIgnoreCase));

            if (componentTaskTemplate is null)
                return ErrorResult
                    .NotFound($"A ComponentTask with the id '{componentTaskDefinition.TaskId}' could not be found for Component {context.Component.Id}.")
                    .ToActionResult();


            if (!string.IsNullOrWhiteSpace(componentTaskDefinition.InputJson))
            {
                var input = JObject.Parse(componentTaskDefinition.InputJson);
                var schema = JSchema.Parse(componentTaskTemplate.InputJsonSchema);

                if (!input.IsValid(schema, out IList<string> schemaErrors))
                    return ErrorResult
                        .BadRequest(new ValidationError { Field = "input", Message = $"ComponentTaskDefinition's input does not match the the Component Task inputJsonSchema.  Errors: {string.Join(", ", schemaErrors)}." })
                        .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync(context.Organization.Id)
                .ConfigureAwait(false);

            var componentTask = new ComponentTask
            {
                Organization = context.Organization.Id,
                ComponentId = context.Component.Id,
                ProjectId = context.Project.Id,
                RequestedBy = currentUser.Id,
                Type = ComponentTaskType.Custom,
                TypeName = componentTaskDefinition.TaskId,

                // component input json is used as a fallback !!!
                InputJson = componentTaskDefinition.InputJson ?? context.Component.InputJson
            };

            var command = new ComponentTaskCreateCommand(currentUser, componentTask);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}
