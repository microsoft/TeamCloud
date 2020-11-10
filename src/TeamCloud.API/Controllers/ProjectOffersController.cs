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
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{org}/projects/{projectId:guid}/offers")]
    [Produces("application/json")]
    public class ProjectOffersController : ApiController
    {
        private readonly IComponentOfferRepository componentOfferRepository;
        private readonly IProjectTemplateRepository projectTemplateRepository;

        public ProjectOffersController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IComponentOfferRepository componentOfferRepository, IProjectTemplateRepository projectTemplateRepository)
            : base(userService, orchestrator, organizationRepository, projectRepository)
        {
            this.componentOfferRepository = componentOfferRepository ?? throw new ArgumentNullException(nameof(componentOfferRepository));
            this.projectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectOffers", Summary = "Gets all Project Offers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Offers", typeof(DataResult<List<ComponentOffer>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided providerId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectAsync(async project =>
        {
            var template = await projectTemplateRepository
                .GetAsync(OrganizationId, project.Template)
                .ConfigureAwait(false);

            var offers = await componentOfferRepository
                .ListAsync(template)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<ComponentOffer>>
                .Ok(offers)
                .ToActionResult();
        });


        [HttpGet("{offerId}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectOfferById", Summary = "Gets the Offer by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a ComponentOffer", typeof(DataResult<ComponentOffer>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ComponentOffer with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string offerId) => EnsureProjectAsync(async project =>
        {
            if (string.IsNullOrWhiteSpace(offerId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var offer = await componentOfferRepository
                .GetAsync(offerId)
                .ConfigureAwait(false);

            if (!project.Type.Providers.Any(p => p.Id.Equals(offer.ProviderId, StringComparison.Ordinal)))
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{offerId}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            if (offer is null)
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{offerId}' could not be found.")
                    .ToActionResult();

            return DataResult<ComponentOffer>
                .Ok(offer)
                .ToActionResult();
        });
    }
}
