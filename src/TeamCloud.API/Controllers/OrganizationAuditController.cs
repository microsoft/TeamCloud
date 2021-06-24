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
using TeamCloud.API.Auth;
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Data.Results;
using TeamCloud.Audit;
using TeamCloud.Audit.Model;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class OrganizationAuditController : TeamCloudController
    {
        private readonly ICommandAuditReader commandAuditReader;

        public OrganizationAuditController(ICommandAuditReader commandAuditReader)
        {
            this.commandAuditReader = commandAuditReader ?? throw new ArgumentNullException(nameof(commandAuditReader));
        }

        [HttpGet("orgs/{organizationId:organizationId}/audit")]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetAuditEntries", Summary = "Gets all audit entries.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns audit entries.", typeof(DataResult<List<CommandAuditEntity>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Organization was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromQuery] string timeRange = null, [FromQuery] string[]? commands = null) => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
        {
            var organizationId = Guid.Parse(context.Organization.Id);

            var timeRangeParsed = TimeSpan.TryParse(timeRange, out var timeRangeTemp) && timeRangeTemp.TotalMinutes >= 1
                ? (TimeSpan?)timeRangeTemp : null; // time range must be at least a second; otherwise, don't use this information

            var entities = await commandAuditReader
                .ListAsync(organizationId, timeRange: timeRangeParsed, commands: commands)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<CommandAuditEntity>>
                .Ok(entities)
                .ToActionResult();
        });

        [HttpGet("orgs/{organizationId:organizationId}/audit/{commandId:commandId}")]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetAuditEntry", Summary = "Gets an audit entry.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns an audit entry.", typeof(DataResult<CommandAuditEntity>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Organization was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get(Guid commandId, [FromQuery] bool expand = false) => ExecuteAsync<TeamCloudOrganizationContext>(async context =>
        {
            var organizationId = Guid.Parse(context.Organization.Id);

            var entity = await commandAuditReader
                .GetAsync(organizationId, commandId, expand)
                .ConfigureAwait(false);

            return DataResult<CommandAuditEntity>
                .Ok(entity)
                .ToActionResult();
        });

        [HttpGet("orgs/{organizationId:organizationId}/audit/commands")]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetAuditCommands", Summary = "Gets all auditable commands.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all auditable commands.", typeof(DataResult<List<string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Organization was not found.", typeof(ErrorResult))]
        public Task<IActionResult> GetAuditCommandTypes() => ExecuteAsync<TeamCloudOrganizationContext>(context =>
        {
            var commands = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.IsDynamic)
                .SelectMany(asm => asm.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(ICommand).IsAssignableFrom(t)))
                .Select(t => t.IsGenericType ? $"{t.Name.Substring(0, t.Name.IndexOf("`", StringComparison.OrdinalIgnoreCase))}<>" : t.Name)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Task.FromResult<IActionResult>(DataResult<List<string>>
                .Ok(commands)
                .ToActionResult());
        });


    }
}
