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
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/schedules")]
    [Produces("application/json")]
    public class ScheduledTasksController : ApiController
    {
        private readonly IScheduledTaskRepository scheduledTaskRepository;
        private readonly IComponentRepository componentRepository;

        private readonly IComponentTemplateRepository componentTemplateRepository;

        public ScheduledTasksController(IScheduledTaskRepository scheduledTaskRepository, IComponentRepository componentRepository, IComponentTemplateRepository componentTemplateRepository) : base()
        {
            this.scheduledTaskRepository = scheduledTaskRepository ?? throw new ArgumentNullException(nameof(scheduledTaskRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetScheduledTasks", Summary = "Gets all Scheduled Tasks.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Scheduled Tasks", typeof(DataResult<List<ScheduledTask>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ExecuteAsync(new Func<User, Organization, Project, Task<IActionResult>>(async (user, organization, project) =>
        {
            var componenetTasks = await scheduledTaskRepository
                .ListAsync(project.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<ScheduledTask>>
                .Ok(componenetTasks)
                .ToActionResult();
        }));


        [HttpGet("{scheduledTaskId}")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetScheduledTask", Summary = "Gets the Scheduled Task.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a Scheduled Task", typeof(DataResult<ScheduledTask>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Scheduled Task with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string scheduledTaskId) => ExecuteAsync(new Func<User, Organization, Project, Task<IActionResult>>(async (user, organization, project) =>
        {
            if (string.IsNullOrWhiteSpace(scheduledTaskId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var scheduledTask = await scheduledTaskRepository
                .GetAsync(project.Id, scheduledTaskId, true)
                .ConfigureAwait(false);

            if (scheduledTask is null)
                return ErrorResult
                    .NotFound($"A Scheduled Task with the id '{scheduledTaskId}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            return DataResult<ScheduledTask>
                .Ok(scheduledTask)
                .ToActionResult();
        }));


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateScheduledTask", Summary = "Creates a new Project Scheduled Task.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Scheduled Task.", typeof(DataResult<ScheduledTask>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Scheduled Task already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ScheduledTaskDefinition scheduledTaskDefinition) => ExecuteAsync(new Func<User, Organization, Project, Task<IActionResult>>(async (user, organization, project) =>
        {
            if (scheduledTaskDefinition is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!scheduledTaskDefinition.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            var componentIds = scheduledTaskDefinition.ComponentTasks
                .Select(ct => ct.ComponentId)
                .Distinct();

            var components = await componentRepository
                .ListAsync(project.Id, componentIds)
                .ToListAsync()
                .ConfigureAwait(false);


            if (!user.IsAdmin(project.Id))
            {
                var notAllowedComponents = components
                    .Where(c => c.Creator != user.Id);

                if (notAllowedComponents.Any())
                    return ErrorResult
                        .BadRequest($"You are not authorized to schedule tasks on the following components: {string.Join(", ", notAllowedComponents.Select(c => c.Id))}.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }


            var notFoundComponentIds = componentIds
                .Where(cid => !components.Any(c => c.Id == cid));

            if (notFoundComponentIds.Any())
                return ErrorResult
                    .BadRequest($"The following components could not be found on this project: {string.Join(", ", notFoundComponentIds)}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var componentTemplateIds = components
                .Select(c => c.TemplateId)
                .Distinct();

            var componentTemplates = await componentTemplateRepository
                .ListAsync(organization.Id, project.Id, componentTemplateIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var notFoundComponentTemplateIds = componentTemplateIds
                .Where(ctid => !componentTemplates.Any(ct => ct.Id == ctid));

            if (notFoundComponentTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following component templates could not be found: {string.Join(", ", notFoundComponentTemplateIds)}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var notFoundComponentTaskTemplateIds = scheduledTaskDefinition.ComponentTasks
                .Where(ctr => !componentTemplates.First(ct => ct.Id == components.First(c => c.Id == ctr.ComponentId).TemplateId).Tasks.Any(t => t.Id == ctr.ComponentTaskTemplateId));

            if (notFoundComponentTaskTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following tasks could not be found on the specified components: {string.Join(", ", notFoundComponentTaskTemplateIds.Select(ctr => ctr.ComponentTaskTemplateId))}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var scheduledTask = new ScheduledTask
            {
                Organization = organization.Id,
                ProjectId = project.Id,
                Enabled = scheduledTaskDefinition.Enabled,
                Recurring = scheduledTaskDefinition.Recurring,
                DaysOfWeek = scheduledTaskDefinition.DaysOfWeek,
                UtcHour = scheduledTaskDefinition.UtcHour,
                UtcMinute = scheduledTaskDefinition.UtcMinute,
                Creator = user.Id,
                ComponentTasks = scheduledTaskDefinition.ComponentTasks
            };

            var command = new ScheduledTaskCreateCommand(user, scheduledTask);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        }));



        [HttpPost("{scheduledTaskId}/run")]
        [Authorize(Policy = AuthPolicies.ProjectScheduledTaskOwner)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "RunScheduledTask", Summary = "Runs a Project Scheduled Task.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Scheduled Task.", typeof(DataResult<ScheduledTask>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Run([FromRoute] string scheduledTaskId) => ExecuteAsync(new Func<User, Organization, Project, ScheduledTask, Task<IActionResult>>(async (user, organization, project, scheduledTask) =>
        {
            var componentIds = scheduledTask.ComponentTasks
                .Select(ct => ct.ComponentId)
                .Distinct();

            var components = await componentRepository
                .ListAsync(project.Id, componentIds)
                .ToListAsync()
                .ConfigureAwait(false);


            if (!user.IsAdmin(project.Id))
            {
                var notAllowedComponents = components
                    .Where(c => c.Creator != user.Id);

                if (notAllowedComponents.Any())
                    return ErrorResult
                        .BadRequest($"You are not authorized to schedule tasks on the following components: {string.Join(", ", notAllowedComponents.Select(c => c.Id))}.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }


            var notFoundComponentIds = componentIds
                .Where(cid => !components.Any(c => c.Id == cid));

            if (notFoundComponentIds.Any())
                return ErrorResult
                    .BadRequest($"The following components could not be found on this project: {string.Join(", ", notFoundComponentIds)}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var componentTemplateIds = components
                .Select(c => c.TemplateId)
                .Distinct();

            var componentTemplates = await componentTemplateRepository
                .ListAsync(organization.Id, project.Id, componentTemplateIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var notFoundComponentTemplateIds = componentTemplateIds
                .Where(ctid => !componentTemplates.Any(ct => ct.Id == ctid));

            if (notFoundComponentTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following component templates could not be found: {string.Join(", ", notFoundComponentTemplateIds)}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var notFoundComponentTaskTemplateIds = scheduledTask.ComponentTasks
                .Where(ctr => !componentTemplates.First(ct => ct.Id == components.First(c => c.Id == ctr.ComponentId).TemplateId).Tasks.Any(t => t.Id == ctr.ComponentTaskTemplateId));

            if (notFoundComponentTaskTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following tasks could not be found on the specified components: {string.Join(", ", notFoundComponentTaskTemplateIds.Select(ctr => ctr.ComponentTaskTemplateId))}.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var command = new ScheduledTaskRunCommand(user, scheduledTask);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        }));

        // TODO: Update (PUT)

        // TODO: Delete (DELETE)
    }
}
