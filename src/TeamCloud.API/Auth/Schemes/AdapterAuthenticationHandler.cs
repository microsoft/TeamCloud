using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DotLiquid;
using Flurl;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamCloud.Adapters;
using TeamCloud.API.Services;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using YamlDotNet.Core.Tokens;

namespace TeamCloud.API.Auth.Schemes
{
    public sealed class AdapterAuthenticationHandler : CookieAuthenticationHandler
    {
        const string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";

        private readonly OneTimeTokenService oneTimeTokenService;
        private readonly IAzureSessionService azureSessionService;
        private readonly IUserRepository userRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IAdapterProvider adapterProvider;

        public AdapterAuthenticationHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, OneTimeTokenService oneTimeTokenService, IAzureSessionService azureSessionService, IUserRepository userRepository, IDeploymentScopeRepository deploymentScopeRepository, IAdapterProvider adapterProvider) : base(options, logger, encoder, clock)
        {
            this.oneTimeTokenService = oneTimeTokenService ?? throw new System.ArgumentNullException(nameof(oneTimeTokenService));
            this.azureSessionService = azureSessionService ?? throw new System.ArgumentNullException(nameof(azureSessionService));
            this.userRepository = userRepository ?? throw new System.ArgumentNullException(nameof(userRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new System.ArgumentNullException(nameof(deploymentScopeRepository));
            this.adapterProvider = adapterProvider ?? throw new System.ArgumentNullException(nameof(adapterProvider));
        }

        protected override async Task InitializeEventsAsync()
        {
            await base
                .InitializeEventsAsync()
                .ConfigureAwait(false);

            Events.OnValidatePrincipal = async (context) =>
            {
                if (context.Request.Query.TryGetValue(AdapterAuthenticationDefaults.QueryParam, out var queryValue))
                {
                    var match = context.Principal.Claims
                        .Any(c => c.Type == AdapterAuthenticationDefaults.ClaimType && c.Value == queryValue.ToString());

                    if (!match)
                        context.RejectPrincipal();
                }
                else
                {
                    var newPrincipal = new ClaimsPrincipal();

                    foreach (var identity in context.Principal.Identities)
                    {
                        if (identity.AuthenticationType.Equals(AdapterAuthenticationDefaults.AuthenticationType))
                        {
                            var newIdentity = new ClaimsIdentity(identity.Claims.Where(c => !c.Type.Equals(ClaimTypes.Role)), AdapterAuthenticationDefaults.AuthenticationType);

                            var userId = identity.Claims.FirstOrDefault(c => c.Type.Equals(ObjectIdClaimType))?.Value;

                            if (!string.IsNullOrEmpty(userId))
                            {
                                var tenantId = await azureSessionService
                                    .GetTenantIdAsync()
                                    .ConfigureAwait(false);

                                newIdentity.AddClaims(await Context
                                    .ResolveClaimsAsync(tenantId.ToString(), userId)
                                    .ConfigureAwait(false));
                            }

                            newPrincipal.AddIdentity(newIdentity);
                        }
                        else
                        {
                            newPrincipal.AddIdentity(identity);
                        }
                    }

                    if (newPrincipal.Identity is null)
                    {
                        context.RejectPrincipal();
                    }
                    else
                    {
                        context.ShouldRenew = true;
                        context.ReplacePrincipal(newPrincipal);
                    }
                }
            };

            Events.OnRedirectToAccessDenied = (context) =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                return context.Response.CompleteAsync();
            };

            Events.OnRedirectToLogin = async (context) =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                if (Request.Query.TryGetValue(AdapterAuthenticationDefaults.QueryParam, out var queryValue))
                {
                    var oneTimeTokenEntity = await oneTimeTokenService
                        .InvalidateTokenAsync(queryValue.ToString())
                        .ConfigureAwait(false);

                    if (oneTimeTokenEntity != null)
                    {
                        var user = await userRepository
                            .GetAsync(oneTimeTokenEntity.OrganizationId.ToString(), oneTimeTokenEntity.UserId.ToString(), expand: true)
                            .ConfigureAwait(false);

                        var tenantId = await azureSessionService
                            .GetTenantIdAsync()
                            .ConfigureAwait(false);

                        var claims = (await Context
                            .ResolveClaimsAsync(tenantId.ToString(), oneTimeTokenEntity.UserId.ToString())
                            .ConfigureAwait(false)).ToList();

                        claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
                        claims.Add(new Claim(ObjectIdClaimType, oneTimeTokenEntity.UserId.ToString()));
                        claims.Add(new Claim(TenantIdClaimType, tenantId.ToString()));
                        claims.Add(new Claim(AdapterAuthenticationDefaults.ClaimType, oneTimeTokenEntity.Token));

                        var claimsIdentity = new ClaimsIdentity(claims, AdapterAuthenticationDefaults.AuthenticationType);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await SignInAsync(claimsPrincipal, context.Properties).ConfigureAwait(false);

                        context.Response.Redirect(OriginalPath);
                    }
                }

                await context.Response
                    .CompleteAsync()
                    .ConfigureAwait(false);
            };
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authenticateResult = await base
                .HandleAuthenticateAsync()
                .ConfigureAwait(false);

            if (authenticateResult.None && Request.RouteValues.TryGetValue("organizationId", out var organizationId) && Request.RouteValues.TryGetValue("deploymentScopeId", out var deploymentScopeId))
            {
                var deploymentScope = await deploymentScopeRepository
                    .GetAsync($"{organizationId}", $"{deploymentScopeId}")
                    .ConfigureAwait(false);

                var adapter = deploymentScope is null
                    ? default(Adapter)
                    : adapterProvider.GetAdapter(deploymentScope.Type);

                if (adapter is IAdapterAuthorize adapterAuthorize)
                {
                    var servicePrincial = await adapterAuthorize
                        .ResolvePrincipalAsync(deploymentScope, Context.Request)
                        .ConfigureAwait(false);

                    if (servicePrincial != null)
                    {
                        var claimsIdentity = servicePrincial.ToClaimsIdentity(AdapterAuthenticationDefaults.AuthenticationType);

                        claimsIdentity.AddClaims(await Context
                            .ResolveClaimsAsync(servicePrincial.TenantId.ToString(), servicePrincial.ObjectId.ToString())
                            .ConfigureAwait(false));

                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        authenticateResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, AdapterAuthenticationDefaults.AuthenticationScheme));
                    }
                }
            }

            return authenticateResult;
        }
    }
}
