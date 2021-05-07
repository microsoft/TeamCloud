/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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


                    options.AddPolicy(AuthPolicies.OrganizationOwner, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy());
                    });

                    options.AddPolicy(AuthPolicies.OrganizationAdmin, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy());
                    });

                    options.AddPolicy(AuthPolicies.OrganizationMember, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           OrganizationUserRole.Member.AuthPolicy());
                    });

                    options.AddPolicy(AuthPolicies.OrganizationRead, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           OrganizationUserRole.Member.AuthPolicy(),
                                           OrganizationUserRole.None.AuthPolicy());
                    });


                    options.AddPolicy(AuthPolicies.ProjectOwner, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           ProjectUserRole.Owner.AuthPolicy());
                    });

                    options.AddPolicy(AuthPolicies.ProjectAdmin, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           ProjectUserRole.Owner.AuthPolicy(),
                                           ProjectUserRole.Admin.AuthPolicy());
                    });

                    options.AddPolicy(AuthPolicies.ProjectMember, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           ProjectUserRole.Owner.AuthPolicy(),
                                           ProjectUserRole.Admin.AuthPolicy(),
                                           ProjectUserRole.Member.AuthPolicy());
                    });


                    options.AddPolicy(AuthPolicies.OrganizationUserWrite, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           UserRolePolicies.UserWritePolicy);
                    });

                    options.AddPolicy(AuthPolicies.ProjectUserWrite, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           ProjectUserRole.Owner.AuthPolicy(),
                                           ProjectUserRole.Admin.AuthPolicy(),
                                           UserRolePolicies.UserWritePolicy);
                    });


                    options.AddPolicy(AuthPolicies.ProjectComponentOwner, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           ProjectUserRole.Owner.AuthPolicy(),
                                           ProjectUserRole.Admin.AuthPolicy(),
                                           UserRolePolicies.ComponentWritePolicy);
                    });


                    options.AddPolicy(AuthPolicies.ProjectScheduledTaskOwner, policy =>
                    {
                        policy.RequireRole(OrganizationUserRole.Owner.AuthPolicy(),
                                           OrganizationUserRole.Admin.AuthPolicy(),
                                           ProjectUserRole.Owner.AuthPolicy(),
                                           ProjectUserRole.Admin.AuthPolicy(),
                                           UserRolePolicies.ScheduledTaskWritePolicy);
                    });
                });

            return services;
        }

        internal static async Task<IEnumerable<Claim>> ResolveClaimsAsync(this HttpContext httpContext, string tenantId, string userId)
        {
            var claims = new List<Claim>();

            if (httpContext.Request.Path.Equals("/orgs", StringComparison.OrdinalIgnoreCase))
                return claims;

            var org = httpContext.RouteValueOrDefault("organizationId");

            if (string.IsNullOrEmpty(org))
                return claims;

            var organizationRepository = httpContext.RequestServices
                .GetRequiredService<IOrganizationRepository>();

            var organizationId = await organizationRepository
                .ResolveIdAsync(tenantId, org)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(organizationId))
                return claims;

            var userRepository = httpContext.RequestServices
                .GetRequiredService<IUserRepository>();

            var user = await userRepository
                .GetAsync(organizationId, userId)
                .ConfigureAwait(false);

            if (user is null)
                return claims;

            claims.Add(new Claim(ClaimTypes.Role, user.AuthPolicy()));

            var orgPath = $"/orgs/{org}";

            if (httpContext.RequestPathStartsWithSegments($"{orgPath}/projects"))
            {
                claims.AddRange(await httpContext.ResolveProjectClaimsAsync(orgPath, user).ConfigureAwait(false));
            }
            else if (httpContext.RequestPathStartsWithSegments($"{orgPath}/users")
                  || httpContext.RequestPathStartsWithSegments($"{orgPath}/me"))
            {
                claims.AddRange(await httpContext.ResolveUserClaimsAsync(orgPath, user).ConfigureAwait(false));
            }

            return claims;
        }

        private static async Task<IEnumerable<Claim>> ResolveProjectClaimsAsync(this HttpContext httpContext, string orgPath, User user)
        {
            var claims = new List<Claim>();

            var project = httpContext.RouteValueOrDefault("projectId");

            if (string.IsNullOrEmpty(project))
                return claims;

            var projectId = await ResolveProjectIdFromRouteAsync(httpContext, user.Organization)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(projectId))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.AuthPolicy(projectId)));

                if (httpContext.RequestPathStartsWithSegments($"{orgPath}/projects/{project}/users"))
                    claims.AddRange(await httpContext.ResolveUserClaimsAsync(orgPath, user).ConfigureAwait(false));

                if (httpContext.RequestPathStartsWithSegments($"{orgPath}/projects/{project}/components"))
                    claims.AddRange(await httpContext.ResolveComponentClaimsAsync(projectId, user).ConfigureAwait(false));

                if (httpContext.RequestPathStartsWithSegments($"{orgPath}/projects/{project}/schedules"))
                    claims.AddRange(await httpContext.ResolveScheduledTaskClaimsAsync(projectId, user).ConfigureAwait(false));
            }

            return claims;
        }

        private static async Task<IEnumerable<Claim>> ResolveUserClaimsAsync(this HttpContext httpContext, string orgPath, User user)
        {
            var claims = new List<Claim>();

            string userId;

            if (httpContext.RequestPathStartsWithSegments($"{orgPath}/me")
            || (httpContext.RequestPathStartsWithSegments(orgPath) && httpContext.RequestPathEndsWith("/me")))
            {
                userId = user.Id;
            }
            else
            {
                userId = await httpContext.ResolveUserIdFromRouteAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(userId) && userId.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, UserRolePolicies.UserWritePolicy));
            }

            return claims;
        }

        private static async Task<IEnumerable<Claim>> ResolveComponentClaimsAsync(this HttpContext httpContext, string projectId, User user)
        {
            var claims = new List<Claim>();

            if (httpContext.Request.Method == HttpMethods.Get)
                return claims;

            var componentId = httpContext.RouteValueOrDefault("ComponentId");

            if (!string.IsNullOrEmpty(componentId))
            {
                var componentRepository = httpContext.RequestServices
                    .GetRequiredService<IComponentRepository>();

                var component = await componentRepository
                    .GetAsync(projectId, componentId)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(component?.Creator) && component.Creator.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    claims.Add(new Claim(ClaimTypes.Role, UserRolePolicies.ComponentWritePolicy));
            }

            return claims;
        }

        private static async Task<IEnumerable<Claim>> ResolveScheduledTaskClaimsAsync(this HttpContext httpContext, string projectId, User user)
        {
            var claims = new List<Claim>();

            if (httpContext.Request.Method == HttpMethods.Get)
                return claims;

            var scheduledTaskId = httpContext.RouteValueOrDefault("ScheduledTaskId");

            if (!string.IsNullOrEmpty(scheduledTaskId))
            {
                var scheduledTaskRepository = httpContext.RequestServices
                    .GetRequiredService<IScheduledTaskRepository>();

                var scheduledTask = await scheduledTaskRepository
                    .GetAsync(projectId, scheduledTaskId)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(scheduledTask?.Creator) && scheduledTask.Creator.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    claims.Add(new Claim(ClaimTypes.Role, UserRolePolicies.ScheduledTaskWritePolicy));
            }

            return claims;
        }

        private static Task<string> ResolveProjectIdFromRouteAsync(this HttpContext httpContext, string organizationId)
        {
            var projectId = httpContext.RouteValueOrDefault("projectId");

            if (string.IsNullOrEmpty(projectId) || projectId.IsGuid())
                return Task.FromResult(projectId);

            var projectsRepository = httpContext.RequestServices
                .GetRequiredService<IProjectRepository>();

            return projectsRepository
                .ResolveIdAsync(organizationId, projectId);
        }

        private static Task<string> ResolveUserIdFromRouteAsync(this HttpContext httpContext)
        {
            var userId = httpContext.RouteValueOrDefault("userId");

            if (string.IsNullOrEmpty(userId) || userId.IsGuid())
                return Task.FromResult(userId);

            var userService = httpContext.RequestServices
                .GetRequiredService<UserService>();

            return userService.GetUserIdAsync(userId);
        }
    }
}
