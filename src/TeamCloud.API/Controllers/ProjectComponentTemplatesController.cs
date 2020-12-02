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
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Git.Services;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{org}/projects/{projectId:projectId}/templates")]
    [Produces("application/json")]
    public class ProjectComponentTemplatesController : ApiController
    {
        private readonly IComponentTemplateRepository componentTemplateRepository;

        public ProjectComponentTemplatesController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IProjectTemplateRepository projectTemplateRepository, IRepositoryService repositoryService, IComponentTemplateRepository componentTemplateRepository)
            : base(userService, orchestrator, organizationRepository, projectRepository, projectTemplateRepository, repositoryService)
        {
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectComponentTemplates", Summary = "Gets all Project Component Templates.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Component Templates", typeof(DataResult<List<ComponentTemplate>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Component Templates with the provided providerId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectAndProjectTemplateAsync(async (project, projectTemplate) =>
        {
            var componenetTemplates = await componentTemplateRepository
                .ListAsync(projectTemplate.Organization, ProjectId)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<ComponentTemplate>>
                .Ok(componenetTemplates)
                .ToActionResult();
        });


        [HttpGet("{id}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectComponentTemplate", Summary = "Gets the Component Template.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a Component Template", typeof(DataResult<ComponentTemplate>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Component Template with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string id) => EnsureProjectAndProjectTemplateAsync(async (project, projectTemplate) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var componentTemplate = await componentTemplateRepository
                .GetAsync(OrgId, ProjectId, id)
                .ConfigureAwait(false);

            if (!(componentTemplate?.ParentId?.Equals(projectTemplate.Id, StringComparison.Ordinal) ?? false))
                return ErrorResult
                    .NotFound($"A Component Template with the id '{id}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            return DataResult<ComponentTemplate>
                .Ok(componentTemplate)
                .ToActionResult();
        });
    }
}
