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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using ValidationError = TeamCloud.API.Data.Results.ValidationError;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/offers")]
    [Produces("application/json")]
    public class ProjectOffersController : ApiController
    {
        private readonly IProjectRepository projectRepository;
        private readonly IComponentOfferRepository componentOfferRepository;

        public ProjectOffersController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IComponentOfferRepository componentOfferRepository) : base(userService, orchestrator)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.componentOfferRepository = componentOfferRepository ?? throw new ArgumentNullException(nameof(componentOfferRepository));
        }

        private async Task<IActionResult> ProcessAsync(Func<ProjectDocument, Task<IActionResult>> callback)
        {
            try
            {
                if (callback is null)
                    throw new ArgumentNullException(nameof(callback));

                if (string.IsNullOrEmpty(ProjectId))
                {
                    return ErrorResult
                        .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                        .ToActionResult();
                }

                var project = await projectRepository
                    .GetAsync(ProjectId)
                    .ConfigureAwait(false);

                if (project is null)
                {
                    return ErrorResult
                        .NotFound($"A Project with the ID '{ProjectId}' could not be found in this TeamCloud Instance.")
                        .ToActionResult();
                }

                return await callback(project)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }


        [HttpGet]
        // TODO: Update auth policy
        [Authorize(Policy = AuthPolicies.ProviderDataRead)]
        [SwaggerOperation(OperationId = "GetProjectOffers", Summary = "Gets all Project Offers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Offers", typeof(DataResult<List<ComponentOffer>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided providerId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ProcessAsync(async project =>
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
        [Authorize(Policy = AuthPolicies.ProviderDataRead)]
        [SwaggerOperation(OperationId = "GetProjectOfferById", Summary = "Gets the Offer by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a ComponentOffer", typeof(DataResult<ComponentOffer>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ComponentOffer with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string offerId) => ProcessAsync(async project =>
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
                    .NotFound($"A ComponentOffer with the id '{offerId}' could not be found for Project {ProjectId}.")
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


        // [HttpPost]
        // [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        // [Consumes("application/json")]
        // [SwaggerOperation(OperationId = "CreateProjectComponent", Summary = "Creates a new Component")]
        // // [SwaggerResponse(StatusCodes.Status201Created, "The new Component was created.", typeof(DataResult<Component>))]
        // [SwaggerResponse(StatusCodes.Status202Accepted, "Started creating the new Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        // [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        // [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found.", typeof(ErrorResult))]
        // [SwaggerResponse(StatusCodes.Status409Conflict, "A ComponentOffer already exists with the id provided in the request body.", typeof(ErrorResult))]
        // public Task<IActionResult> Post([FromBody] ComponentRequest request) => ProcessAsync(async project =>
        // {
        //     if (request is null)
        //         return ErrorResult
        //             .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
        //             .ToActionResult();

        //     // if (!request.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
        //     //     return ErrorResult
        //     //         .BadRequest(validationResult)
        //     //         .ToActionResult();

        //     var offerDocument = await componentOfferRepository
        //         .GetAsync(request.OfferId)
        //         .ConfigureAwait(false);

        //     if (offerDocument is null)
        //         return ErrorResult
        //             .NotFound($"A ComponentOffer with the id '{request.OfferId}' could not be found for Project {ProjectId}.")
        //             .ToActionResult();

        //     if (!JObject.FromObject(request.Input).IsValid(JSchema.Parse(offerDocument.InputJsonSchema), out IList<string> schemaErrors))
        //         return ErrorResult
        //             .BadRequest(new ValidationError { Field = "input", Message = $"ComponentRequest's input does not match the the Offer inputJsonSchema.  Errors: {string.Join(", ", schemaErrors)}." })
        //             .ToActionResult();

        //     var currentUser = await UserService
        //         .CurrentUserAsync()
        //         .ConfigureAwait(false);

        //     var component = new ComponentDocument
        //     {
        //         OfferId = offerDocument.Id,
        //         ProjectId = project.Id,
        //         ProviderId = offerDocument.ProviderId,
        //         RequesterId = currentUser.Id,
        //         Input = request.Input
        //     };

        //     return await Orchestrator
        //         .InvokeAndReturnActionResultAsync<ComponentRequest, Component>(new OrchestratorComponentRequestCommand(currentUser, request, project.Id), Request)
        //         .ConfigureAwait(false);
        // });


        // [HttpPut("{offerId}")]
        // [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        // [Consumes("application/json")]
        // [SwaggerOperation(OperationId = "UpdateProjectOffer", Summary = "Updates an existing ComponentOffer.")]
        // [SwaggerResponse(StatusCodes.Status200OK, "The ComponentOffer was updated.", typeof(DataResult<ComponentOffer>))]
        // [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        // [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found, or a ComponentOffer with the provided offerId was not found.", typeof(ErrorResult))]
        // public Task<IActionResult> Put([FromRoute] string offerId, [FromBody] ComponentOffer offer) => ProcessAsync(async project =>
        // {
        //     if (string.IsNullOrWhiteSpace(offerId))
        //         return ErrorResult
        //             .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
        //             .ToActionResult();

        //     if (offer is null)
        //         return ErrorResult
        //             .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
        //             .ToActionResult();

        //     if (!offer.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
        //         return ErrorResult
        //             .BadRequest(validationResult)
        //             .ToActionResult();

        //     if (!offer.Id.Equals(offerId, StringComparison.Ordinal))
        //         return ErrorResult
        //             .BadRequest(new ValidationError { Field = "id", Message = $"ComponentOffer's id does match the identifier provided in the path." })
        //             .ToActionResult();

        //     var offerDocument = await componentOfferRepository
        //         .GetAsync(offer.Id)
        //         .ConfigureAwait(false);

        //     if (offerDocument is null)
        //         return ErrorResult
        //             .NotFound($"A ComponentOffer with the id '{offer.Id}' could not be found for Project {ProjectId}.")
        //             .ToActionResult();

        //     offerDocument.PopulateFromExternalModel(offer);

        //     var currentUser = await UserService
        //         .CurrentUserAsync()
        //         .ConfigureAwait(false);

        //     return await Orchestrator
        //         .InvokeAndReturnActionResultAsync<ComponentOfferDocument, ComponentOffer>(new OrchestratorComponentOfferUpdateCommand(currentUser, offerDocument), Request)
        //         .ConfigureAwait(false);
        // });


        // [HttpDelete("{offerId}")]
        // [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        // [SwaggerOperation(OperationId = "DeleteProviderData", Summary = "Deletes a ProviderData.")]
        // [SwaggerResponse(StatusCodes.Status204NoContent, "The ProviderData was deleted.", typeof(DataResult<ProviderData>))]
        // [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        // [SwaggerResponse(StatusCodes.Status404NotFound, "A ProviderData with the providerDataId provided was not found.", typeof(ErrorResult))]
        // public Task<IActionResult> Delete([FromRoute] string offerId) => ProcessAsync(async project =>
        // {
        //     if (string.IsNullOrWhiteSpace(offerId))
        //         return ErrorResult
        //             .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
        //             .ToActionResult();

        //     var offerDocument = await componentOfferRepository
        //         .GetAsync(offerId)
        //         .ConfigureAwait(false);

        //     if (offerDocument is null)
        //         return ErrorResult
        //             .NotFound($"A ComponentOffer with the id '{offerDocument.Id}' could not be found.")
        //             .ToActionResult();

        //     var currentUser = await UserService
        //         .CurrentUserAsync()
        //         .ConfigureAwait(false);

        //     return await Orchestrator
        //         .InvokeAndReturnActionResultAsync<ComponentOfferDocument, ComponentOffer>(new OrchestratorComponentOfferDeleteCommand(currentUser, offerDocument), Request)
        //         .ConfigureAwait(false);
        // });
    }
}
