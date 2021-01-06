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
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/components/{componentId:componentId}/deployments")]
    [Produces("application/json")]
    public class ProjectDeploymentsController : ApiController
    {
        private readonly IComponentDeploymentRepository componentDeploymentRepository;

        public ProjectDeploymentsController(IComponentDeploymentRepository componentDeploymentRepository) : base()
        {
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetProjectDeployments", Summary = "Gets all Project Component Deployments.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Component Deployments", typeof(DataResult<List<ComponentDeployment>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Component Deployments with the provided providerId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ExecuteAsync(new Func<User, Organization, Project, Component, Task<IActionResult>>(async (user, organization, project, component) =>
        {
            var componenetDeployments = await componentDeploymentRepository
                .ListAsync(component.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<ComponentDeployment>>
                .Ok(componenetDeployments)
                .ToActionResult();
        }));


        [HttpGet("{id}")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetProjectDeployment", Summary = "Gets the Component Template.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a Component Template", typeof(DataResult<ComponentDeployment>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Component Template with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string id) => ExecuteAsync(new Func<User, Organization, Project, Component, Task<IActionResult>>(async (user, organization, project, component) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var componentDeployment = await componentDeploymentRepository
                .GetAsync(component.Id, id, true)
                .ConfigureAwait(false);

            if (componentDeployment is null)
                return ErrorResult
                    .NotFound($"A Component Deployment with the id '{id}' could not be found for Component {component.Id}.")
                    .ToActionResult();

            return DataResult<ComponentDeployment>
                .Ok(componentDeployment)
                .ToActionResult();
        }));
    }
}
