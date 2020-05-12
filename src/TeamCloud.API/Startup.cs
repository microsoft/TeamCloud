/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TeamCloud.API.Middleware;
using TeamCloud.API.Routing;
using TeamCloud.API.Services;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Deployment.Providers;
using TeamCloud.Azure.Directory;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Data;
using TeamCloud.Data.CosmosDb;
using TeamCloud.Http;
using TeamCloud.Model.Data;

namespace TeamCloud.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

#pragma warning disable CA1822 // Mark members as static

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseWhen(context => !(context.Request.Path.StartsWithSegments("/api/admin/users", StringComparison.OrdinalIgnoreCase)
                                     && HttpMethods.IsPost(context.Request.Method)), appBuilder =>
            {
                // ensure TeamCloud to be configured for all paths other than /api/admin/users
                appBuilder.UseMiddleware<EnsureTeamCloudUserMiddleware>();
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamCloud API v1");
            });

            app.UseHttpsRedirection()
               .UseRouting()
               .UseAuthentication()
               .UseAuthorization()
               .UseEndpoints(endpoints => endpoints.MapControllers());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMemoryCache()
                .AddTeamCloudOptions(Assembly.GetExecutingAssembly())
                .AddTeamCloudOptionsShared()
                .AddTeamCloudAzure(configuration =>
                {
                    configuration
                        .AddDirectory()
                        .AddDeployment()
                        .SetDeploymentArtifactsProvider<AzureStorageArtifactsProvider>();
                })
                .AddTeamCloudHttp(configuration =>
                {
                    // nothing to configure
                });

            services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IUsersRepositoryReadOnly, CosmosDbUsersRepository>()
                .AddSingleton<IProjectsRepositoryReadOnly, CosmosDbProjectsRepository>()
                .AddSingleton<ITeamCloudRepositoryReadOnly, CosmosDbTeamCloudRepository>()
                .AddSingleton<IProjectTypesRepositoryReadOnly, CosmosDbProjectTypesRepository>()
                .AddSingleton<IClientErrorFactory, ClientErrorFactory>()
                .AddSingleton<Orchestrator>()
                .AddSingleton<UserService>()
                .AddScoped<EnsureTeamCloudUserMiddleware>();

            ConfigureAuthentication(services);
            ConfigureAuthorization(services);

            services
                .AddApplicationInsightsTelemetry();

            services
                .AddMvc();

            services
                .AddRouting(options =>
                {
                    options.ConstraintMap.Add("userNameOrId", typeof(UserIdentifierRouteConstraint));
                    options.ConstraintMap.Add("projectNameOrId", typeof(ProjectIdentifierRouteConstraint));
                })
                .AddControllers()
                .AddNewtonsoftJson()
                .ConfigureApiBehaviorOptions(options => options.SuppressMapClientErrors = true);

#pragma warning disable CA1308 // Normalize strings to uppercase

            ValidatorOptions.DisplayNameResolver = (type, memberInfo, lambda) => memberInfo?.Name?.ToLowerInvariant();
            ValidatorOptions.PropertyNameResolver = (type, memberInfo, lambda) => memberInfo?.Name?.ToLowerInvariant();

#pragma warning restore CA1308 // Normalize strings to uppercase

            ConfigureSwagger(services);
        }

#pragma warning restore CA1822 // Mark members as static

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services
                .AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "TeamCloud",
                        Description = "API for working with a TeamCloud instance.",
                        Contact = new OpenApiContact
                        {
                            Url = new Uri("https://github.com/microsoft/TeamCloud/issues/new"),
                            Email = @"Markus.Heiliger@microsoft.com",
                            Name = "TeamCloud Dev Team"
                        },
                        License = new OpenApiLicense
                        {
                            Name = "TeamCloud is licensed under the MIT License",
                            Url = new Uri("https://github.com/microsoft/TeamCloud/blob/master/LICENSE")
                        }
                    });

                    // options.AddFluentValidationRules();
                    options.EnableAnnotations();
                    options.UseInlineDefinitionsForEnums();

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" },
                            },
                            new [] { "default", "admin", "projectCreate", "projectRead", "projectDelete" }
                        }
                    });

                    options.OperationFilter<SecurityRequirementsOperationFilter>();
                })
                .AddSwaggerGenNewtonsoftSupport(); // explicit Newtonsoft opt-in - needs to be placed after AddSwaggerGen()
        }

        private static void ConfigureAuthentication(IServiceCollection services)
        {
            var resourceManagerOptions = services
                .BuildServiceProvider()
                .GetRequiredService<AzureResourceManagerOptions>();

            services
                .AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                .AddAzureADBearer(options =>
                {
                    options.Instance = AzureEnvironment.AzureGlobalCloud.AuthenticationEndpoint;
                    options.TenantId = resourceManagerOptions.TenantId;
                });

            services
                .AddHttpContextAccessor()
                .Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
                {
                    // This is an Microsoft identity platform Web API
                    options.Authority += "/v2.0";

                    // Disable audience validation
                    options.TokenValidationParameters.ValidateAudience = false;

                    // The valid issuers can be based on Azure identity V1 or V2
                    options.TokenValidationParameters.ValidIssuers = new string[]
                    {
                        $"https://login.microsoftonline.com/{resourceManagerOptions.TenantId}/v2.0",
                        $"https://sts.windows.net/{resourceManagerOptions.TenantId}/"
                    };

                    options.Events = new JwtBearerEvents()
                    {
                        OnTokenValidated = async (TokenValidatedContext context) =>
                        {
                            var userId = context.Principal.GetObjectId();

                            var userClaims = await ResolveClaimsAsync(userId, context.HttpContext).ConfigureAwait(false);
                            if (userClaims.Any()) context.Principal.AddIdentity(new ClaimsIdentity(userClaims));
                        }
                    };
                });
        }

        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services
                .AddMvc(options =>
                {
                    // Requires authentication across the API
                    options.Filters.Add(new AuthorizeFilter("default"));
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
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName());
                    });

                    options.AddPolicy("projectCreate", policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           TeamCloudUserRole.Creator.PolicyRoleName());
                    });

                    options.AddPolicy("projectRead", policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProjectUserRole.Owner.PolicyRoleName(),
                                           ProjectUserRole.Member.PolicyRoleName());
                    });

                    options.AddPolicy("projectDelete", policy =>
                    {
                        policy.RequireRole(TeamCloudUserRole.Admin.PolicyRoleName(),
                                           ProjectUserRole.Owner.PolicyRoleName());
                    });
                });
        }

        private static async Task<IEnumerable<Claim>> ResolveClaimsAsync(Guid userId, HttpContext httpContext)
        {
            var claims = new List<Claim>();

            var userReepository = httpContext.RequestServices
                .GetRequiredService<IUsersRepositoryReadOnly>();

            var user = await userReepository
                .GetAsync(userId)
                .ConfigureAwait(false);

            claims.Add(new Claim(ClaimTypes.Role, user.Role.PolicyRoleName()));

            if (httpContext.Request.Path.StartsWithSegments("/api/projects", StringComparison.OrdinalIgnoreCase))
            {
                var projectIdRouteValue = httpContext.GetRouteData()
                    .Values.GetValueOrDefault("ProjectId", StringComparison.OrdinalIgnoreCase)?.ToString();

                if (Guid.TryParse(projectIdRouteValue, out Guid projectId))
                {
                    claims.Add(new Claim(ClaimTypes.Role, user.RoleFor(projectId).PolicyRoleName()));
                }
            }

            return claims;
        }
    }
}
