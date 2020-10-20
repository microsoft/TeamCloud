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
    [Route("api/projects/{projectId:guid}/offers")]
    [Produces("application/json")]
    public class ProjectOffersController : ApiController
    {
        private readonly IComponentOfferRepository componentOfferRepository;

        public ProjectOffersController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IComponentOfferRepository componentOfferRepository)
            : base(userService, orchestrator, projectRepository)
        {
            this.componentOfferRepository = componentOfferRepository ?? throw new ArgumentNullException(nameof(componentOfferRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectOffers", Summary = "Gets all Project Offers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Offers", typeof(DataResult<List<ComponentOffer>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided providerId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectAsync(async project =>
        {
            var offerDocuments = await componentOfferRepository
                .ListAsync(project.Type.Providers.Select(p => p.Id))
                .ToListAsync()
                .ConfigureAwait(false);

            var offers = offerDocuments.Select(o => o.PopulateExternalModel()).ToList();

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

            var offerDocument = await componentOfferRepository
                .GetAsync(offerId)
                .ConfigureAwait(false);

            if (!project.Type.Providers.Any(p => p.Id.Equals(offerDocument.ProviderId, StringComparison.Ordinal)))
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{offerId}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            if (offerDocument is null)
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{offerId}' could not be found.")
                    .ToActionResult();

            var offer = offerDocument.PopulateExternalModel();

            return DataResult<ComponentOffer>
                .Ok(offer)
                .ToActionResult();
        });
    }
}
