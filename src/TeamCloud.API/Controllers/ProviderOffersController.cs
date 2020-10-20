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
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/providers/{providerId:providerId}/offers")]
    [Produces("application/json")]
    public class ProviderOffersController : ApiController
    {
        private readonly IComponentOfferRepository componentOfferRepository;

        public ProviderOffersController(UserService userService, Orchestrator orchestrator, IProviderRepository providerRepository, IComponentOfferRepository componentOfferRepository)
            : base(userService, orchestrator, providerRepository)
        {
            this.componentOfferRepository = componentOfferRepository ?? throw new ArgumentNullException(nameof(componentOfferRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProviderOfferRead)]
        [SwaggerOperation(OperationId = "GetProviderOffers", Summary = "Gets all Provider Offers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Provider Offers", typeof(DataResult<List<ComponentOffer>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProviderAsync(async provider =>
        {
            var offerDocuments = await componentOfferRepository
                .ListAsync(provider.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var offers = offerDocuments.Select(o => o.PopulateExternalModel()).ToList();

            return DataResult<List<ComponentOffer>>
                .Ok(offers)
                .ToActionResult();
        });


        [HttpGet("{offerId}")]
        [Authorize(Policy = AuthPolicies.ProviderOfferRead)]
        [SwaggerOperation(OperationId = "GetProviderOfferById", Summary = "Gets the Offer by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a ComponentOffer", typeof(DataResult<ComponentOffer>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ComponentOffer with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string offerId) => EnsureProviderAsync(async provider =>
        {
            if (string.IsNullOrWhiteSpace(offerId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var offerDocument = await componentOfferRepository
                .GetAsync(offerId)
                .ConfigureAwait(false);

            if (offerDocument is null)
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{offerId}' could not be found.")
                    .ToActionResult();

            var offer = offerDocument.PopulateExternalModel();

            return DataResult<ComponentOffer>
                .Ok(offer)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProviderOfferWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProviderOffer", Summary = "Creates a new ComponentOffer item")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new ComponentOffer was created.", typeof(DataResult<ComponentOffer>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A ComponentOffer already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ComponentOffer offer) => EnsureProviderAsync(async provider =>
        {
            if (offer is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!offer.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            if (!offer.ProviderId.Equals(provider.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "providerId", Message = $"ComponentOffer's providerId does match the providerId provided in the path." })
                    .ToActionResult();

            var offerDocument = await componentOfferRepository
                .GetAsync(offer.Id)
                .ConfigureAwait(false);

            if (offerDocument != null)
                return ErrorResult
                    .Conflict($"A ComponentOffer with the id '{offerDocument.Id}' already exists.")
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            offerDocument = new ComponentOfferDocument()
                .PopulateFromExternalModel(offer);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ComponentOfferDocument, ComponentOffer>(new OrchestratorComponentOfferCreateCommand(currentUser, offerDocument), Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{offerId}")]
        [Authorize(Policy = AuthPolicies.ProviderOfferWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProviderOffer", Summary = "Updates an existing ComponentOffer.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The ComponentOffer was updated.", typeof(DataResult<ComponentOffer>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found, or a ComponentOffer with the provided offerId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Put([FromRoute] string offerId, [FromBody] ComponentOffer offer) => EnsureProviderAsync(async provider =>
        {
            if (string.IsNullOrWhiteSpace(offerId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (offer is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!offer.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            if (!offer.Id.Equals(offerId, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"ComponentOffer's id does match the identifier provided in the path." })
                    .ToActionResult();

            if (!offer.ProviderId.Equals(provider.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "providerId", Message = $"ComponentOffer's providerId does match the providerId provided in the path." })
                    .ToActionResult();

            var offerDocument = await componentOfferRepository
                .GetAsync(offer.Id)
                .ConfigureAwait(false);

            if (offerDocument is null)
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{offer.Id}' could not be found.")
                    .ToActionResult();

            offerDocument.PopulateFromExternalModel(offer);

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ComponentOfferDocument, ComponentOffer>(new OrchestratorComponentOfferUpdateCommand(currentUser, offerDocument), Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{offerId}")]
        [Authorize(Policy = AuthPolicies.ProviderOfferWrite)]
        [SwaggerOperation(OperationId = "DeleteProviderOffer", Summary = "Deletes a ComponentOffer.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The ComponentOffer was deleted.", typeof(DataResult<ComponentOffer>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ComponentOffer with the offerId provided was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string offerId) => EnsureProviderAsync(async provider =>
        {
            if (string.IsNullOrWhiteSpace(offerId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var offerDocument = await componentOfferRepository
                .GetAsync(offerId)
                .ConfigureAwait(false);

            if (offerDocument is null)
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{offerDocument.Id}' could not be found.")
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ComponentOfferDocument, ComponentOffer>(new OrchestratorComponentOfferDeleteCommand(currentUser, offerDocument), Request)
                .ConfigureAwait(false);
        });
    }
}
