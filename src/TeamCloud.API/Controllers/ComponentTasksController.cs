/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

using ValidationError = TeamCloud.API.Data.Results.ValidationError;

namespace TeamCloud.API.Controllers;

[ApiController]
[Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/components/{componentId:componentId}/tasks")]
[Produces("application/json")]
public class ComponentTasksController : TeamCloudController
{
    private readonly IComponentTaskRepository componentTaskRepository;
    private readonly IComponentTemplateRepository componentTemplateRepository;

    public ComponentTasksController(IComponentTaskRepository componentTaskRepository,
                                    IComponentTemplateRepository componentTemplateRepository,
                                    IValidatorProvider validatorProvider) : base(validatorProvider)
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
    public Task<IActionResult> Get() => WithContextAsync<Component>(async (contextUser, component) =>
   {
       var componenetTasks = await componentTaskRepository
           .ListAsync(component.Id)
           .ToListAsync()
           .ConfigureAwait(false);

       return DataResult<List<ComponentTask>>
           .Ok(componenetTasks)
           .ToActionResult();
   });


    [HttpGet("{taskId}")]
    [Authorize(Policy = AuthPolicies.ProjectMember)]
    [SwaggerOperation(OperationId = "GetComponentTask", Summary = "Gets the Component Task.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns a Component Task", typeof(DataResult<ComponentTask>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "A Component Task with the provided id was not found.", typeof(ErrorResult))]
    public Task<IActionResult> Get([FromRoute] string taskId) => WithContextAsync<ComponentTask>(async (contextUser, componentTask) =>
    {
        componentTask = await componentTaskRepository
            .ExpandAsync(componentTask, true)
            .ConfigureAwait(false);

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
    public Task<IActionResult> Post([FromBody] ComponentTaskDefinition componentTaskDefinition) => WithContextAsync<Project, Component>(async (contextUser, project, component) =>
    {
        if (componentTaskDefinition is null)
            return ErrorResult
                .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                .ToActionResult();

        if (!componentTaskDefinition.TryValidate(ValidatorProvider, out var validationResult))
            return ErrorResult
                .BadRequest(validationResult)
                .ToActionResult();

        var componentTemplate = await componentTemplateRepository
            .GetAsync(component.Organization, component.ProjectId, component.TemplateId)
            .ConfigureAwait(false);

        if (componentTemplate is null || !componentTemplate.ParentId.Equals(project.Template, StringComparison.OrdinalIgnoreCase))
            return ErrorResult
                .NotFound($"A ComponentTemplate with the id '{component.TemplateId}' could not be found for Project {project.Id}.")
                .ToActionResult();

        var componentTaskTemplate = componentTemplate.Tasks.FirstOrDefault(t => t.Id.Equals(componentTaskDefinition.TaskId, StringComparison.OrdinalIgnoreCase));

        if (componentTaskTemplate is null)
            return ErrorResult
                .NotFound($"A ComponentTask with the id '{componentTaskDefinition.TaskId}' could not be found for Component {component.Id}.")
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

        var componentTask = new ComponentTask
        {
            Organization = component.Organization,
            ComponentId = component.Id,
            ProjectId = component.ProjectId,
            RequestedBy = contextUser.Id,
            Type = ComponentTaskType.Custom,
            TypeName = componentTaskDefinition.TaskId,

            // component input json is used as a fallback !!!
            InputJson = componentTaskDefinition.InputJson ?? component.InputJson
        };

        var command = new ComponentTaskCreateCommand(contextUser, componentTask);

        return await Orchestrator
            .InvokeAndReturnActionResultAsync(command, Request)
            .ConfigureAwait(false);
    });


    [HttpPut("{taskId}/cancel")]
    [Authorize(Policy = AuthPolicies.ProjectMember)]
    [SwaggerOperation(OperationId = "CancelComponentTask", Summary = "Rerun a Project Component Task.")]
    [SwaggerResponse(StatusCodes.Status200OK, "The canceled Project Component Task.", typeof(DataResult<ComponentTask>))]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Starts cancelling the Project Component Task. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "A Project or Project Component with the provided identifier was not found.", typeof(ErrorResult))]
    public Task<IActionResult> Cancel() => WithContextAsync<ComponentTask>(async (contextUser, componentTask) =>
    {
        if (componentTask.Type != ComponentTaskType.Custom)
            return ErrorResult
                .BadRequest($"Component tasks of type '{componentTask.TypeName}' cannot be canceled!", ResultErrorCode.ValidationError)
                .ToActionResult();

        if (componentTask.TaskState.IsFinal())
            return ErrorResult
                .BadRequest($"Component tasks in state '{componentTask.TaskState}' cannot be canceled!", ResultErrorCode.ValidationError)
                .ToActionResult();

        var command = new ComponentTaskCancelCommand(contextUser, componentTask);

        return await Orchestrator
            .InvokeAndReturnActionResultAsync(command, Request)
            .ConfigureAwait(false);
    });

    [HttpPut("{taskId}/rerun")]
    [Authorize(Policy = AuthPolicies.ProjectMember)]
    [SwaggerOperation(OperationId = "ReRunComponentTask", Summary = "Cancel an active Project Component Task.")]
    [SwaggerResponse(StatusCodes.Status201Created, "The created Project Component Task created by the rerun action.", typeof(DataResult<ComponentTask>))]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Rerun the Project Component Task. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "A Project or Project Component with the provided identifier was not found.", typeof(ErrorResult))]
    public Task<IActionResult> Rerun() => WithContextAsync<ComponentTask>(async (contextUser, componentTask) =>
    {
        if (componentTask.Type != ComponentTaskType.Custom)
            return ErrorResult
                .BadRequest($"Component tasks of type '{componentTask.TypeName}' cannot be restarted!", ResultErrorCode.ValidationError)
                .ToActionResult();

        if (componentTask.TaskState.IsActive())
            return ErrorResult
                .BadRequest($"Component tasks in state '{componentTask.TaskState}' cannot be restarted!", ResultErrorCode.ValidationError)
                .ToActionResult();

        componentTask = componentTask.Clone(true) as ComponentTask;

        var command = new ComponentTaskCreateCommand(contextUser, componentTask);

        // CAUTION - this is a PUT opertion on the API side but behaves like a POST
        // when it comes to the returned response, as a new component task instance
        // is created. therefore we are going to enforce the POST behaviour.

        return await Orchestrator
        .InvokeAndReturnActionResultAsync(command, HttpMethod.Post)
        .ConfigureAwait(false);
    });
}
