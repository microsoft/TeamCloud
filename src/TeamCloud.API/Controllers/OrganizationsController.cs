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
using TeamCloud.API;
using TeamCloud.API.Auth;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Data.Validators;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class OrganizationsController : ApiController
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IUserRepository userRepository;

        public OrganizationsController(IOrganizationRepository organizationRepository, IUserRepository userRepository) : base()
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }


        [HttpGet("orgs")]
        [Authorize(Policy = AuthPolicies.Default)]
        [SwaggerOperation(OperationId = "GetOrganizations", Summary = "Gets all Organizations.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Organizations.", typeof(DataResult<List<Organization>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var orgIds = await userRepository
                .ListOrgsAsync(UserService.CurrentUserId)
                .ToListAsync()
                .ConfigureAwait(false);

            var orgs = !orgIds.Any()
                ? new List<Organization>()
                : await organizationRepository
                    .ListAsync(UserService.CurrentUserTenant, orgIds)
                    .ToListAsync()
                    .ConfigureAwait(false);

            return DataResult<List<Organization>>
                .Ok(orgs)
                .ToActionResult();
        }


        [HttpGet("orgs/{organizationId:organizationId}")]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetOrganization", Summary = "Gets an Organization.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns an Organization.", typeof(DataResult<Organization>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "An Organization with the provided identifier was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get([FromRoute] string org) => ExecuteAsync((user, organization) =>
        {
            return DataResult<Organization>
                .Ok(organization)
                .ToActionResultAsync();
        });


        [HttpPost("orgs")]
        [Authorize(Policy = AuthPolicies.Default)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateOrganization", Summary = "Creates a new Organization.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new Organization was created.", typeof(DataResult<Organization>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] OrganizationDefinition organizationDefinition)
        {
            if (organizationDefinition is null)
                throw new ArgumentNullException(nameof(organizationDefinition));

            var validation = new OrganizationDefinitionValidator().Validate(organizationDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var organization = await organizationRepository
                .GetAsync(UserService.CurrentUserTenant, organizationDefinition.Slug)
                .ConfigureAwait(false);

            if (organization != null)
                return ErrorResult
                    .Conflict($"The Organication '{organizationDefinition.Slug}' already exists. Please try your request again with a unique Organization Name or Id.")
                    .ToActionResult();

            organization = new Organization
            {
                Id = Guid.NewGuid().ToString(),
                Tenant = UserService.CurrentUserTenant,
                DisplayName = organizationDefinition.DisplayName,
                SubscriptionId = organizationDefinition.SubscriptionId,
                Location = organizationDefinition.Location
            };

            var currentUser = await UserService
                .CurrentUserAsync(null, allowUnsafe: true)
                .ConfigureAwait(false);

            currentUser.Role = OrganizationUserRole.Owner;
            currentUser.Organization = organization.Id;

            var command = new OrganizationCreateCommand(currentUser, organization);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        }


        // [HttpPut("orgs/{organizationId:organizationId}")]
        // [Authorize(Policy = AuthPolicies.Admin)]
        // [Consumes("application/json")]
        // [SwaggerOperation(OperationId = "UpdateOrganization", Summary = "Updates an existing Organization.")]
        // [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Organization. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        // [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        // [SwaggerResponse(StatusCodes.Status404NotFound, "An Organization with the ID provided in the request body was not found.", typeof(ErrorResult))]
        // [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        // public Task<IActionResult> Put([FromRoute] string org, [FromBody] User user) => EnsureUserAsync(async userDocument =>
        // {
        //     if (user is null)
        //         throw new ArgumentNullException(nameof(user));

        //     var validation = new UserValidator().Validate(user);

        //     if (!validation.IsValid)
        //         return ErrorResult
        //             .BadRequest(validation)
        //             .ToActionResult();

        //     if (userDocument.IsAdmin() && !user.IsAdmin())
        //     {
        //         var otherAdmins = await OrganizationRepository
        //             .ListAdminsAsync()
        //             .AnyAsync(a => a.Id != user.Id)
        //             .ConfigureAwait(false);

        //         if (!otherAdmins)
        //             return ErrorResult
        //                 .BadRequest($"The TeamCloud instance must have at least one Admin user. To change this user's role you must first add another Admin user.", ResultErrorCode.ValidationError)
        //                 .ToActionResult();
        //     }

        //     if (!userDocument.HasEqualMemberships(user))
        //         return ErrorResult
        //             .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships can not be changed using the TeamCloud (system) users API. To update a user's project memberships use the project users API." })
        //             .ToActionResult();

        //     var currentUser = await UserService
        //         .CurrentUserAsync()
        //         .ConfigureAwait(false);

        //     userDocument.PopulateFromExternalModel(user);

        //     var command = new OrganizationUpdateCommand(currentUser, userDocument);

        //     return await Orchestrator
        //         .InvokeAndReturnActionResultAsync<User, User>(command, Request)
        //         .ConfigureAwait(false);
        // });


        [HttpDelete("orgs/{organizationId:organizationId}")]
        [Authorize(Policy = AuthPolicies.OrganizationOwner)]
        [SwaggerOperation(OperationId = "DeleteOrganization", Summary = "Deletes an existing Organization.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Organization. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "An Organization with the identifier provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string organizationId) => ExecuteAsync(async (user, organization) =>
        {
            var command = new OrganizationDeleteCommand(user, organization);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}
