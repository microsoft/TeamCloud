using System;
using System.Threading.Tasks;
using Flurl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.Adapters;
using TeamCloud.Adapters.Authorization;
using TeamCloud.API.Auth;
using TeamCloud.API.Auth.Schemes;
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.API.Swagger;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers;

[ApiController]
[Route("orgs/{organizationId:organizationId}/scopes/{deploymentScopeId:deploymentScopeId}/authorize")]
public class DeploymentScopesAuthorizationController : TeamCloudController
{
    private readonly OneTimeTokenService oneTimeTokenService;
    private readonly IAdapterProvider adapterProvider;
    private readonly IAuthorizationEndpointsResolver authorizationEndpointsResolver;

    public DeploymentScopesAuthorizationController(OneTimeTokenService oneTimeTokenService, IAdapterProvider adapterProvider, IAuthorizationEndpointsResolver authorizationEndpointsResolver) : base()
    {
        this.oneTimeTokenService = oneTimeTokenService ?? throw new ArgumentNullException(nameof(oneTimeTokenService));
        this.adapterProvider = adapterProvider ?? throw new ArgumentNullException(nameof(adapterProvider));
        this.authorizationEndpointsResolver = authorizationEndpointsResolver ?? throw new ArgumentNullException(nameof(authorizationEndpointsResolver));
    }

    [HttpGet()]
    [HttpPost()]
    [Authorize(Policy = AuthPolicies.AdapterAuthorizationFlow)]
    [Produces("text/html")]
    [SwaggerIgnore]
    public Task<IActionResult> Authorize() => WithContextAsync<DeploymentScope>(async (contextUser, deploymentScope) =>
    {
        if (adapterProvider.GetAdapter(deploymentScope.Type) is IAdapterAuthorize adapterAuthorize)
        {
            var authorizationEndpoints = await authorizationEndpointsResolver
                .GetAuthorizationEndpointsAsync(deploymentScope)
                .ConfigureAwait(false);

            return await adapterAuthorize
                .HandleAuthorizeAsync(deploymentScope, Request, authorizationEndpoints)
                .ConfigureAwait(false);
        }
        else
        {
            return ErrorResult.NotFound($"Could not find authorize endpoint for {deploymentScope}").ToActionResult();
        }
    });


    [HttpGet("callback")]
    [HttpPost("callback")]
    [Authorize(Policy = AuthPolicies.AdapterAuthorizationFlow)]
    //[AllowAnonymous]
    [Produces("text/html")]
    [SwaggerIgnore]
    public Task<IActionResult> Callback() => WithContextAsync<DeploymentScope>(async (contextUser, deploymentScope) =>
    {
        if (adapterProvider.GetAdapter(deploymentScope.Type) is IAdapterAuthorize adapterAuthorize)
        {
            var authorizationEndpoints = await authorizationEndpointsResolver
                .GetAuthorizationEndpointsAsync(deploymentScope)
                .ConfigureAwait(false);

            return await adapterAuthorize
                .HandleCallbackAsync(deploymentScope, Request, authorizationEndpoints)
                .ConfigureAwait(false);
        }
        else
        {
            return ErrorResult.NotFound($"Could not find authorize endpoint for {deploymentScope}").ToActionResult();
        }
    });

    [HttpGet("initialize")]
    [Authorize(Policy = AuthPolicies.AdapterAuthorizationInit)]
    [Produces("application/json")]
    [SwaggerOperation(OperationId = "InitializeAuthorization", Summary = "Initialize a new authorization session for a deployment scope.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the DeploymentScope that was initialized for an authorization session", typeof(DataResult<DeploymentScope>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
    public Task<IActionResult> Initialize() => WithContextAsync<DeploymentScope>(async (contextUser, deploymentScope) =>
    {
        if (!deploymentScope.Authorizable)
            return ErrorResult
                .BadRequest($"Deployment scope {deploymentScope} doesn't support authorization", ResultErrorCode.ServerError)
                .ToActionResult();

        var token = await oneTimeTokenService
            .AcquireTokenAsync(contextUser, TimeSpan.FromMinutes(1))
            .ConfigureAwait(false);

        deploymentScope.AuthorizeUrl = deploymentScope.AuthorizeUrl
            .SetQueryParam(AdapterAuthenticationDefaults.QueryParam, token)
            .ToString();

        return DataResult<DeploymentScope>
            .Ok(deploymentScope)
            .ToActionResult();
    });
}
