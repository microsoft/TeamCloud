/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Auth
{
    internal static class AuthExtensions
    {

        internal static IServiceCollection AddTeamCloudAuthorization(this IServiceCollection services)
        {
            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy(AuthPolicies.Default, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                    });

                    options.AddPolicy(AuthPolicies.Admin, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName());
                    });

                    options.AddPolicy(AuthPolicies.UserRead, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           UserRolePolicies.UserReadPolicy,
                                           UserRolePolicies.UserWritePolicy);
                    });

                    options.AddPolicy(AuthPolicies.UserWrite, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           UserRolePolicies.UserWritePolicy);
                    });

                    // options.AddPolicy(AuthPolicies.ProjectUserRead, policy =>
                    // {
                    //     policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                    //                        ProjectUserRole.Owner.PolicyRoleName(),
                    //                        UserRolePolicies.UserReadPolicy,
                    //                        UserRolePolicies.UserWritePolicy);
                    // });

                    options.AddPolicy(AuthPolicies.ProjectUserWrite, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProjectUserRole.Owner.PolicyRoleName(),
                                           UserRolePolicies.UserWritePolicy);
                    });

                    options.AddPolicy(AuthPolicies.ProjectLinkWrite, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProjectUserRole.Owner.PolicyRoleName(),
                                           ProjectUserRole.Provider.PolicyRoleName());
                    });

                    options.AddPolicy(AuthPolicies.ProjectRead, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProjectUserRole.Owner.PolicyRoleName(),
                                           ProjectUserRole.Member.PolicyRoleName(),
                                           ProjectUserRole.Provider.PolicyRoleName());
                    });

                    options.AddPolicy(AuthPolicies.ProjectWrite, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProjectUserRole.Owner.PolicyRoleName());
                    });

                    options.AddPolicy(AuthPolicies.ProjectCreate, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           TeamCloudUserRole.Creator.PolicyRoleName());
                    });

                    options.AddPolicy(AuthPolicies.ProjectIdentityRead, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProjectUserRole.Owner.PolicyRoleName(),
                                           ProjectUserRole.Provider.PolicyRoleName());
                    });

                    options.AddPolicy(AuthPolicies.ProviderDataRead, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProviderUserRoles.ProviderReadPolicyRoleName,
                                           ProviderUserRoles.ProviderWritePolicyRoleName);
                    });

                    options.AddPolicy(AuthPolicies.ProviderDataWrite, policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProviderUserRoles.ProviderWritePolicyRoleName);
                    });
                });

            return services;
        }

        internal static async Task<IEnumerable<Claim>> ResolveClaimsAsync(this HttpContext httpContext, string userId)
        {
            var claims = new List<Claim>();

            var userRepository = httpContext.RequestServices
                .GetRequiredService<IUserRepository>();

            var user = await userRepository
                .GetAsync(userId)
                .ConfigureAwait(false);

            if (user is null)
                return claims;

            claims.Add(new Claim(ClaimTypes.Role, user.Role.PolicyRoleName()));

            if (httpContext.Request.Path.StartsWithSegments("/api/projects", StringComparison.OrdinalIgnoreCase))
            {
                claims.AddRange(await httpContext.ResolveProjectClaimsAsync(user).ConfigureAwait(false));
            }
            else if (httpContext.Request.Path.StartsWithSegments("/api/users", StringComparison.OrdinalIgnoreCase)
                  || httpContext.Request.Path.StartsWithSegments("/api/me", StringComparison.OrdinalIgnoreCase))
            {
                claims.AddRange(await httpContext.ResolveUserClaimsAsync(user).ConfigureAwait(false));
            }
            else if (httpContext.Request.Path.StartsWithSegments("/api/providers", StringComparison.OrdinalIgnoreCase))
            {
                claims.AddRange(await httpContext.ResolveProviderClaimsAsync(user).ConfigureAwait(false));
            }

            return claims;
        }

        private static async Task<IEnumerable<Claim>> ResolveProjectClaimsAsync(this HttpContext httpContext, UserDocument user)
        {
            var claims = new List<Claim>();

            var projectIdRouteValue = httpContext.GetRouteData()
                .Values.GetValueOrDefault("ProjectId", StringComparison.OrdinalIgnoreCase)?.ToString();

            if (string.IsNullOrEmpty(projectIdRouteValue))
                projectIdRouteValue = await httpContext.ResolveProjectIdFromNameOrIdRouteAsync().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(projectIdRouteValue))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.RoleFor(projectIdRouteValue).PolicyRoleName()));

                if (httpContext.Request.Path.StartsWithSegments($"/api/projects/{projectIdRouteValue}/users", StringComparison.OrdinalIgnoreCase))
                    claims.AddRange(await httpContext.ResolveUserClaimsAsync(user).ConfigureAwait(false));

                if (httpContext.Request.Path.StartsWithSegments($"/api/projects/{projectIdRouteValue}/providers", StringComparison.OrdinalIgnoreCase))
                    claims.AddRange(await httpContext.ResolveProviderClaimsAsync(user).ConfigureAwait(false));
            }

            return claims;
        }

        private static async Task<IEnumerable<Claim>> ResolveUserClaimsAsync(this HttpContext httpContext, UserDocument user)
        {
            var claims = new List<Claim>();

            string userIdRouteValue;

            if (httpContext.Request.Path.StartsWithSegments("/api/me", StringComparison.OrdinalIgnoreCase)
            || httpContext.Request.Path.Value.EndsWith("/me", StringComparison.OrdinalIgnoreCase))
            {
                userIdRouteValue = user.Id;
            }
            else
            {
                userIdRouteValue = httpContext.GetRouteData()
                    .Values.GetValueOrDefault("UserId", StringComparison.OrdinalIgnoreCase)?.ToString();

                if (string.IsNullOrEmpty(userIdRouteValue))
                    userIdRouteValue = await httpContext.ResolveUserIdFromNameOrIdRouteAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(userIdRouteValue) && userIdRouteValue.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, UserRolePolicies.UserWritePolicy));
            }

            return claims;
        }

        private static async Task<IEnumerable<Claim>> ResolveProviderClaimsAsync(this HttpContext httpContext, UserDocument user)
        {
            var claims = new List<Claim>();

            var providerIdRouteValue = httpContext.GetRouteData()
                .Values.GetValueOrDefault("ProviderId", StringComparison.OrdinalIgnoreCase)?.ToString();

            if (!string.IsNullOrEmpty(providerIdRouteValue) && user.UserType == UserType.Provider)
            {
                var providerRepository = httpContext.RequestServices
                    .GetRequiredService<IProviderRepository>();

                var provider = await providerRepository
                    .GetAsync(providerIdRouteValue)
                    .ConfigureAwait(false);

                if (provider?.PrincipalId.HasValue ?? false
                 && provider.PrincipalId.Value.ToString().Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    claims.Add(new Claim(ClaimTypes.Role, ProviderUserRoles.ProviderWritePolicyRoleName));
            }

            return claims;
        }

        private static async Task<string> ResolveProjectIdFromNameOrIdRouteAsync(this HttpContext httpContext)
        {
            var projectNameOrIdRouteValue = httpContext.GetRouteData()
                .Values.GetValueOrDefault("ProjectNameOrId", StringComparison.OrdinalIgnoreCase)?.ToString();

            if (string.IsNullOrEmpty(projectNameOrIdRouteValue) || projectNameOrIdRouteValue.IsGuid())
                return projectNameOrIdRouteValue;

            var projectssRepository = httpContext.RequestServices
                .GetRequiredService<IProjectRepository>();

            var project = await projectssRepository
                .GetAsync(projectNameOrIdRouteValue)
                .ConfigureAwait(false);

            return project?.Id;
        }

        private static async Task<string> ResolveUserIdFromNameOrIdRouteAsync(this HttpContext httpContext)
        {
            var userNameOrIdRouteValue = httpContext.GetRouteData()
                .Values.GetValueOrDefault("UsertNameOrId", StringComparison.OrdinalIgnoreCase)?.ToString();

            if (string.IsNullOrEmpty(userNameOrIdRouteValue))
                return userNameOrIdRouteValue;

            var userService = httpContext.RequestServices
                .GetRequiredService<UserService>();

            var userId = await userService.GetUserIdAsync(userNameOrIdRouteValue)
                .ConfigureAwait(false);

            return userId;
        }
    }
}
