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
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/schedules")]
    [Produces("application/json")]
    public class ScheduleController : TeamCloudController
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly IComponentRepository componentRepository;

        private readonly IComponentTemplateRepository componentTemplateRepository;

        public ScheduleController(IScheduleRepository scheduleRepository, IComponentRepository componentRepository, IComponentTemplateRepository componentTemplateRepository) : base()
        {
            this.scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetSchedules", Summary = "Gets all Schedule.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Schedule", typeof(DataResult<List<Schedule>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            var componenetTasks = await scheduleRepository
                .ListAsync(context.Project.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<Schedule>>
                .Ok(componenetTasks)
                .ToActionResult();
        });


        [HttpGet("{scheduleId}")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetSchedule", Summary = "Gets the Schedule.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a Schedule", typeof(DataResult<Schedule>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Schedule with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string scheduleId) => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            if (string.IsNullOrWhiteSpace(scheduleId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var schedule = await scheduleRepository
                .GetAsync(context.Project.Id, scheduleId, true)
                .ConfigureAwait(false);

            if (schedule is null)
                return ErrorResult
                    .NotFound($"A Schedule with the id '{scheduleId}' could not be found for Project {context.Project.Id}.")
                    .ToActionResult();

            return DataResult<Schedule>
                .Ok(schedule)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateSchedule", Summary = "Creates a new Project Schedule.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Schedule.", typeof(DataResult<Schedule>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Schedule already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ScheduleDefinition scheduleDefinition) => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            if (scheduleDefinition is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!scheduleDefinition.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            var componentIds = scheduleDefinition.ComponentTasks
                .Select(ct => ct.ComponentId)
                .Distinct();

            var components = await componentRepository
                .ListAsync(context.Project.Id, componentIds)
                .ToListAsync()
                .ConfigureAwait(false);


            if (!context.ContextUser.IsAdmin(context.Project.Id))
            {
                var notAllowedComponents = components
                    .Where(c => c.Creator != context.ContextUser.Id);

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
                .ListAsync(context.Organization.Id, context.Project.Id, componentTemplateIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var notFoundComponentTemplateIds = componentTemplateIds
                .Where(ctid => !componentTemplates.Any(ct => ct.Id == ctid));

            if (notFoundComponentTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following component templates could not be found: {string.Join(", ", notFoundComponentTemplateIds)}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var notFoundComponentTaskTemplateIds = scheduleDefinition.ComponentTasks
                .Where(ctr => !componentTemplates.First(ct => ct.Id == components.First(c => c.Id == ctr.ComponentId).TemplateId).Tasks.Any(t => t.Id == ctr.ComponentTaskTemplateId));

            if (notFoundComponentTaskTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following tasks could not be found on the specified components: {string.Join(", ", notFoundComponentTaskTemplateIds.Select(ctr => ctr.ComponentTaskTemplateId))}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var schedule = new Schedule
            {
                Organization = context.Organization.Id,
                ProjectId = context.Project.Id,
                Enabled = scheduleDefinition.Enabled,
                Recurring = scheduleDefinition.Recurring,
                DaysOfWeek = scheduleDefinition.DaysOfWeek,
                UtcHour = scheduleDefinition.UtcHour,
                UtcMinute = scheduleDefinition.UtcMinute,
                Creator = context.ContextUser.Id,
                ComponentTasks = scheduleDefinition.ComponentTasks
            };

            var command = new ScheduleCreateCommand(context.ContextUser, schedule);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{scheduleId}")]
        [Authorize(Policy = AuthPolicies.ProjectScheduleOwner)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateSchedule", Summary = "Updates a Project Schedule.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The updated Project Schedule.", typeof(DataResult<Schedule>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Schedule id provided in the could not be found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string scheduleId, [FromBody] Schedule updatedSchedule) => ExecuteAsync<TeamCloudScheduleContext>(async context =>
        {
            if (updatedSchedule is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!updatedSchedule.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();


            if (!context.Schedule.Id.Equals(updatedSchedule.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"Schedule's id does match the identifier provided in the path." })
                    .ToActionResult();


            var componentIds = updatedSchedule.ComponentTasks
                .Select(ct => ct.ComponentId)
                .Distinct();

            var components = await componentRepository
                .ListAsync(context.Project.Id, componentIds)
                .ToListAsync()
                .ConfigureAwait(false);


            if (!context.ContextUser.IsAdmin(context.Project.Id))
            {
                var notAllowedComponents = components
                    .Where(c => c.Creator != context.ContextUser.Id);

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
                .ListAsync(context.Organization.Id, context.Project.Id, componentTemplateIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var notFoundComponentTemplateIds = componentTemplateIds
                .Where(ctid => !componentTemplates.Any(ct => ct.Id == ctid));

            if (notFoundComponentTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following component templates could not be found: {string.Join(", ", notFoundComponentTemplateIds)}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var notFoundComponentTaskTemplateIds = updatedSchedule.ComponentTasks
                .Where(ctr => !componentTemplates.First(ct => ct.Id == components.First(c => c.Id == ctr.ComponentId).TemplateId).Tasks.Any(t => t.Id == ctr.ComponentTaskTemplateId));

            if (notFoundComponentTaskTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following tasks could not be found on the specified components: {string.Join(", ", notFoundComponentTaskTemplateIds.Select(ctr => ctr.ComponentTaskTemplateId))}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            if (updatedSchedule.Creator != context.Schedule.Creator)
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "creator", Message = $"The Schedule's creator field cannot be changed." })
                    .ToActionResult();

            updatedSchedule.LastUpdatedBy = context.ContextUser.Id;
            updatedSchedule.LastUpdated = DateTime.UtcNow;

            updatedSchedule.Created = context.Schedule.Created;
            updatedSchedule.LastRun = context.Schedule.LastRun;

            var command = new ScheduleUpdateCommand(context.ContextUser, updatedSchedule);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPost("{scheduleId}/run")]
        [Authorize(Policy = AuthPolicies.ProjectScheduleOwner)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "RunSchedule", Summary = "Runs a Project Schedule.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The Project Schedule run.", typeof(DataResult<Schedule>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Run([FromRoute] string scheduleId) => ExecuteAsync<TeamCloudScheduleContext>(async context =>
        {
            var componentIds = context.Schedule.ComponentTasks
                .Select(ct => ct.ComponentId)
                .Distinct();

            var components = await componentRepository
                .ListAsync(context.Project.Id, componentIds)
                .ToListAsync()
                .ConfigureAwait(false);


            if (!context.ContextUser.IsAdmin(context.Project.Id))
            {
                var notAllowedComponents = components
                    .Where(c => c.Creator != context.ContextUser.Id);

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
                .ListAsync(context.Organization.Id, context.Project.Id, componentTemplateIds)
                .ToListAsync()
                .ConfigureAwait(false);

            var notFoundComponentTemplateIds = componentTemplateIds
                .Where(ctid => !componentTemplates.Any(ct => ct.Id == ctid));

            if (notFoundComponentTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following component templates could not be found: {string.Join(", ", notFoundComponentTemplateIds)}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var notFoundComponentTaskTemplateIds = context.Schedule.ComponentTasks
                .Where(ctr => !componentTemplates.First(ct => ct.Id == components.First(c => c.Id == ctr.ComponentId).TemplateId).Tasks.Any(t => t.Id == ctr.ComponentTaskTemplateId));

            if (notFoundComponentTaskTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following tasks could not be found on the specified components: {string.Join(", ", notFoundComponentTaskTemplateIds.Select(ctr => ctr.ComponentTaskTemplateId))}.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var command = new ScheduleRunCommand(context.ContextUser, context.Schedule);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });

        // TODO: Delete (DELETE)
    }
}
