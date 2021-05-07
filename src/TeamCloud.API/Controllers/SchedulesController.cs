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
    public class ScheduleController : ApiController
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
        public Task<IActionResult> Get() => ExecuteAsync(new Func<User, Organization, Project, Task<IActionResult>>(async (user, organization, project) =>
        {
            var componenetTasks = await scheduleRepository
                .ListAsync(project.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<Schedule>>
                .Ok(componenetTasks)
                .ToActionResult();
        }));


        [HttpGet("{scheduleId}")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetSchedule", Summary = "Gets the Schedule.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a Schedule", typeof(DataResult<Schedule>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Schedule with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string scheduleId) => ExecuteAsync(new Func<User, Organization, Project, Task<IActionResult>>(async (user, organization, project) =>
        {
            if (string.IsNullOrWhiteSpace(scheduleId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var schedule = await scheduleRepository
                .GetAsync(project.Id, scheduleId, true)
                .ConfigureAwait(false);

            if (schedule is null)
                return ErrorResult
                    .NotFound($"A Schedule with the id '{scheduleId}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            return DataResult<Schedule>
                .Ok(schedule)
                .ToActionResult();
        }));


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateSchedule", Summary = "Creates a new Project Schedule.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Schedule.", typeof(DataResult<Schedule>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Schedule already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ScheduleDefinition scheduleDefinition) => ExecuteAsync(new Func<User, Organization, Project, Task<IActionResult>>(async (user, organization, project) =>
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


            var notFoundComponentTaskTemplateIds = scheduleDefinition.ComponentTasks
                .Where(ctr => !componentTemplates.First(ct => ct.Id == components.First(c => c.Id == ctr.ComponentId).TemplateId).Tasks.Any(t => t.Id == ctr.ComponentTaskTemplateId));

            if (notFoundComponentTaskTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following tasks could not be found on the specified components: {string.Join(", ", notFoundComponentTaskTemplateIds.Select(ctr => ctr.ComponentTaskTemplateId))}.", ResultErrorCode.ValidationError)
                    .ToActionResult();


            var schedule = new Schedule
            {
                Organization = organization.Id,
                ProjectId = project.Id,
                Enabled = scheduleDefinition.Enabled,
                Recurring = scheduleDefinition.Recurring,
                DaysOfWeek = scheduleDefinition.DaysOfWeek,
                UtcHour = scheduleDefinition.UtcHour,
                UtcMinute = scheduleDefinition.UtcMinute,
                Creator = user.Id,
                ComponentTasks = scheduleDefinition.ComponentTasks
            };

            var command = new ScheduleCreateCommand(user, schedule);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        }));



        [HttpPost("{scheduleId}/run")]
        [Authorize(Policy = AuthPolicies.ProjectScheduleOwner)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "RunSchedule", Summary = "Runs a Project Schedule.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Schedule.", typeof(DataResult<Schedule>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Run([FromRoute] string scheduleId) => ExecuteAsync(new Func<User, Organization, Project, Schedule, Task<IActionResult>>(async (user, organization, project, schedule) =>
        {
            var componentIds = schedule.ComponentTasks
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


            var notFoundComponentTaskTemplateIds = schedule.ComponentTasks
                .Where(ctr => !componentTemplates.First(ct => ct.Id == components.First(c => c.Id == ctr.ComponentId).TemplateId).Tasks.Any(t => t.Id == ctr.ComponentTaskTemplateId));

            if (notFoundComponentTaskTemplateIds.Any())
                return ErrorResult
                    .BadRequest($"The following tasks could not be found on the specified components: {string.Join(", ", notFoundComponentTaskTemplateIds.Select(ctr => ctr.ComponentTaskTemplateId))}.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var command = new ScheduleRunCommand(user, schedule);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        }));

        // TODO: Update (PUT)

        // TODO: Delete (DELETE)
    }
}
