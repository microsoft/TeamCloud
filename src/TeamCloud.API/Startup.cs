/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TeamCloud.Data;
using TeamCloud.Model;

namespace TeamCloud.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions(Assembly.GetExecutingAssembly());

            services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IProjectsContainer, ProjectsContainer>()
                .AddSingleton<ITeamCloudContainer, TeamCloudContainer>()
                .AddSingleton<Orchestrator>();

            services
                .AddMvc(options =>
                {
                    // Requires authentication across the API
                    options.Filters.Add(new AuthorizeFilter("default"));
                });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/";

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/",

                        ValidateAudience = true,
                        ValidAudience = "https://microsoft.onmicrosoft.com/TeamCloud.Proxy",

                        ValidateLifetime = true
                    };

                    options.Events = new JwtBearerEvents()
                    {
                        OnTokenValidated = async (TokenValidatedContext context) =>
                        {
                            var userObjectId = context.Principal.GetObjectId();

                            var userRoleClaims = new List<Claim>()
                            {
                                // TODO: remove this
                                // just for testing - everyone is an admin
                                new Claim(ClaimTypes.Role, UserRoles.TeamCloud.Admin)
                            };

                            var projectIdRoute = context
                                .HttpContext.GetRouteData()
                                .Values.GetValueOrDefault("ProjectId", StringComparison.OrdinalIgnoreCase)?.ToString();

                            if (Guid.TryParse(projectIdRoute, out Guid projectId))
                            {
                                var projectRepository = context.HttpContext.RequestServices
                                    .GetRequiredService<IProjectsContainer>();

                                var project = await projectRepository
                                    .GetAsync(projectId)
                                    .ConfigureAwait(false);

                                var projectClaims = (project.Users ?? Enumerable.Empty<User>())
                                    .Where(user => user.Id == userObjectId)
                                    .Select(user => new Claim(ClaimTypes.Role, user.Role));

                                userRoleClaims.AddRange(projectClaims);
                            }

                            if (userRoleClaims.Any())
                            {
                                var userIdentity = new ClaimsIdentity(userRoleClaims);

                                context.Principal.AddIdentity(userIdentity);
                            }
                        }
                    };
                });

            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy("default", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                    });

                    options.AddPolicy("admin", policy =>
                    {
                        policy.RequireRole(UserRoles.TeamCloud.Admin);
                    });

                    options.AddPolicy("projectCreate", policy =>
                    {
                        policy.RequireRole(UserRoles.TeamCloud.Admin, UserRoles.TeamCloud.Creator);
                    });

                    options.AddPolicy("projectRead", policy =>
                    {
                        policy.RequireRole(UserRoles.TeamCloud.Admin, UserRoles.Project.Owner, UserRoles.Project.Member);
                    });

                    options.AddPolicy("projectDelete", policy =>
                    {
                        policy.RequireRole(UserRoles.TeamCloud.Admin, UserRoles.Project.Owner);
                    });
                });

            services
                .AddControllers()
                .AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection()
               .UseRouting()
               .UseAuthentication()
               .UseAuthorization()
               .UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}