/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using Microsoft.OpenApi.Models;
using TeamCloud.API.Auth;
using TeamCloud.API.Initialization;
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
using TeamCloud.Data.Caching;
using TeamCloud.Data.CosmosDb;
using TeamCloud.Git.Services;
using TeamCloud.Http;

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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AzureResourceManagerOptions resourceManagerOptions)
        {
            if (env.IsDevelopment())
            {
                app
                    .UseDeveloperExceptionPage()
                    .UseCors(builder => builder
                        .SetIsOriginAllowed(origin => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            }
            else
            {
                app
                    .UseHsts()
                    .UseHttpsRedirection();
            }

            app
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/openapi/v1/openapi.json", "TeamCloud API v1");
                    c.OAuthClientId(resourceManagerOptions.ClientId);
                    c.OAuthClientSecret("");
                    c.OAuthUsePkce();
                });

            app
                .UseRouting()
                .UseAuthentication()
                .UseMiddleware<EnsureTeamCloudModelMiddleware>()
                .UseMiddleware<RequestResponseTracingMiddleware>()
               // .UseWhen(context => context.Request.RequiresAdminUserSet(), appBuilder =>
               // {
               //     appBuilder.UseMiddleware<EnsureTeamCloudAdminMiddleware>();
               // })
               .UseAuthorization()
               .UseEndpoints(endpoints => endpoints.MapControllers());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddTeamCloudOptions(Assembly.GetExecutingAssembly())
                .AddTeamCloudOptionsShared()
                .AddTeamCloudAzure(configuration =>
                {
                    configuration
                        .AddDirectory()
                        .AddDeployment()
                        .SetDeploymentArtifactsProvider<AzureStorageArtifactsProvider>();
                })
                .AddTeamCloudHttp();

            if (string.IsNullOrEmpty(Configuration.GetValue<string>("Cache:Configuration")))
            {
                services
                    .AddDistributedMemoryCache()
                    .AddSingleton<IContainerDocumentCache, ContainerDocumentCache>();
            }
            else
            {
                services
                    .AddDistributedRedisCache(options => Configuration.Bind("Cache", options))
                    .AddSingleton<IContainerDocumentCache, ContainerDocumentCache>();
            }

            services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IOrganizationRepository, CosmosDbOrganizationRepository>()
                .AddSingleton<IUserRepository, CosmosDbUserRepository>()
                .AddSingleton<IDeploymentScopeRepository, CosmosDbDeploymentScopeRepository>()
                .AddSingleton<IProjectTemplateRepository, CosmosDbProjectTemplateRepository>()
                .AddSingleton<IComponentTemplateRepository, CosmosDbComponentTemplateRepository>()
                .AddSingleton<IProjectRepository, CosmosDbProjectRepository>()
                .AddSingleton<IComponentRepository, CosmosDbComponentRepository>()
                // .AddSingleton<IProjectLinkRepository, CosmosDbProjectLinkRepository>()
                .AddSingleton<IClientErrorFactory, ClientErrorFactory>()
                .AddSingleton<Orchestrator>()
                .AddSingleton<UserService>()
                .AddSingleton<IRepositoryService, RepositoryService>()
                .AddScoped<EnsureTeamCloudModelMiddleware>()
                .AddScoped<RequestResponseTracingMiddleware>();
            // .AddScoped<EnsureTeamCloudAdminMiddleware>()
            // .AddTransient<IHostInitializer, TeamCloudAdminInitializer>();

            services
                .AddSingleton<RecyclableMemoryStreamManager>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton(provider => provider.GetRequiredService<ObjectPoolProvider>().Create(new StringBuilderPooledObjectPolicy()));

            ConfigureAuthentication(services);
            ConfigureAuthorization(services);

            services
                .AddApplicationInsightsTelemetry();

            services
                .AddMvc();

            services
                .AddRouting(options =>
                {
                    options.ConstraintMap.Add("userId", typeof(UserIdentifierRouteConstraint));
                    options.ConstraintMap.Add("projectId", typeof(ProjectIdentifierRouteConstraint));
                    // options.ConstraintMap.Add("providerId", typeof(ProviderIdentifierRouteConstraint));
                })
                .AddControllers()
                .AddNewtonsoftJson()
                .ConfigureApiBehaviorOptions(options => options.SuppressMapClientErrors = true);

#pragma warning disable CA1308 // Normalize strings to uppercase

            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, lambda) => memberInfo?.Name?.ToLowerInvariant();
            ValidatorOptions.Global.PropertyNameResolver = (type, memberInfo, lambda) => memberInfo?.Name?.ToLowerInvariant();

#pragma warning restore CA1308 // Normalize strings to uppercase

            ConfigureSwagger(services);
        }

#pragma warning restore CA1822 // Mark members as static

        private static void ConfigureSwagger(IServiceCollection services)
        {
            var resourceManagerOptions = services
                .BuildServiceProvider()
                .GetRequiredService<AzureResourceManagerOptions>();

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

                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                TokenUrl = new Uri($"https://login.microsoftonline.com/{resourceManagerOptions.TenantId}/oauth2/v2.0/token"),
                                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{resourceManagerOptions.TenantId}/oauth2/v2.0/authorize"),
                                Scopes = new Dictionary<string, string> {
                                    { "openid", "Sign you in" },
                                    { "http://TeamCloud.aztcclitestsix/user_impersonation", "Access the TeamCloud API" }
                                }
                            }
                        }
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" },
                            },
                            new [] { "openid", "http://TeamCloud.aztcclitestsix/user_impersonation" }
                        }
                    });

                    options.OperationFilter<SecurityRequirementsOperationFilter>();
                })
                .AddSwaggerGenNewtonsoftSupport();
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

                            var userClaims = await context.HttpContext.ResolveClaimsAsync(userId).ConfigureAwait(false);
                            if (userClaims.Any()) context.Principal.AddIdentity(new ClaimsIdentity(userClaims));
                        }
                    };
                });
        }

        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services // Requires authentication across the API
                .AddMvc(options => options.Filters.Add(new AuthorizeFilter(AuthPolicies.Default)));

            services
                .AddTeamCloudAuthorization();
        }
    }
}
