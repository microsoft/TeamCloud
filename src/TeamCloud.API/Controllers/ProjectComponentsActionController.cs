/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/components/{componentId:componentId}")]
    [Produces("application/json")]
    public class ProjectComponentsActionController : ApiController
    {
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;

        public ProjectComponentsActionController(IComponentRepository componentRepository, IComponentTemplateRepository componentTemplateRepository, IDeploymentScopeRepository deploymentScopeRepository) : base()
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        }

        [HttpPost(nameof(Reset))]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "ResetProjectComponent", Summary = "Reset a Project Component.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Reset a Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Component with the provided componentId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Reset() => ExecuteAsync(new Func<User, Organization, Project, Component, Task<IActionResult>>(async (user, organization, project, component) =>
        {
            var command = new ComponentResetCommand(user, component);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        }));
    }
}

