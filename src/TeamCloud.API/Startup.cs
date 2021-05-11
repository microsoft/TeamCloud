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
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using Microsoft.OpenApi.Models;
using TeamCloud.Adapters;
using TeamCloud.Adapters.AzureDevOps;
using TeamCloud.Adapters.AzureResourceManager;
using TeamCloud.Adapters.GitHub;
using TeamCloud.API.Auth;
using TeamCloud.API.Middleware;
using TeamCloud.API.Routing;
using TeamCloud.API.Services;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Deployment.Providers;
using TeamCloud.Azure.Directory;
using TeamCloud.Azure.Resources;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Data;
using TeamCloud.Data.CosmosDb;
using TeamCloud.Data.Expanders;
using TeamCloud.Data.Providers;
using TeamCloud.Git.Caching;
using TeamCloud.Git.Services;
using TeamCloud.Http;
using TeamCloud.Serialization.Encryption;

namespace TeamCloud.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamCloud API v1");
                    c.OAuthClientId(resourceManagerOptions.ClientId);
                    c.OAuthClientSecret("");
                    c.OAuthUsePkce();
                });

            app
                .UseRouting()
                .UseAuthentication()
                .UseMiddleware<EnsureTeamCloudModelMiddleware>()
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
                        .AddResources()
                        .AddDeployment()
                        .SetDeploymentArtifactsProvider<AzureStorageArtifactsProvider>();
                })
                .AddTeamCloudHttp();

            if (string.IsNullOrEmpty(Configuration.GetValue<string>("Cache:Configuration")))
            {
                services
                    .AddDistributedMemoryCache()
                    .AddSingleton<IRepositoryCache, RepositoryCache>();
            }
            else
            {
                services
                    .AddDistributedRedisCache(options => Configuration.Bind("Cache", options))
                    .AddSingleton<IRepositoryCache, RepositoryCache>();
            }

            if (Configuration.TryBind<EncryptionOptions>("Encryption", out var encryptionOptions) && CloudStorageAccount.TryParse(encryptionOptions.KeyStorage, out var keyStorageAccount))
            {
                const string EncryptionContainerName = "encryption";

                keyStorageAccount
                    .CreateCloudBlobClient()
                    .GetContainerReference(EncryptionContainerName)
                    .CreateIfNotExistsAsync().Wait();

                var dataProtectionBuilder = services
                    .AddDataProtection()
                    .SetApplicationName("TeamCloud")
                    .PersistKeysToAzureBlobStorage(encryptionOptions.KeyStorage, EncryptionContainerName, "keys.xml");

                if (!string.IsNullOrEmpty(encryptionOptions.KeyVault))
                {
                    //dataProtectionBuilder.ProtectKeysWithAzureKeyVault()
                    throw new NotImplementedException();
                }

                EncryptedValueProvider.DefaultDataProtectionProvider = services.BuildServiceProvider().GetDataProtectionProvider();
            }

            services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IOrganizationRepository, CosmosDbOrganizationRepository>()
                .AddSingleton<IUserRepository, CosmosDbUserRepository>()
                .AddSingleton<IDeploymentScopeRepository, CosmosDbDeploymentScopeRepository>()
                .AddSingleton<IProjectIdentityRepository, CosmosDbProjectIdentityRepository>()
                .AddSingleton<IProjectTemplateRepository, CosmosDbProjectTemplateRepository>()
                .AddSingleton<IComponentTemplateRepository, CosmosDbComponentTemplateRepository>()
                .AddSingleton<IProjectRepository, CosmosDbProjectRepository>()
                .AddSingleton<IComponentRepository, CosmosDbComponentRepository>()
                .AddSingleton<IComponentTaskRepository, CosmosDbComponentTaskRepository>()
                .AddSingleton<IClientErrorFactory, ClientErrorFactory>()
                .AddSingleton<OrchestratorService>()
                .AddSingleton<UserService>()
                .AddSingleton<IRepositoryService, RepositoryService>()
                .AddScoped<EnsureTeamCloudModelMiddleware>();

            services
                .AddSingleton<RecyclableMemoryStreamManager>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton(provider => provider.GetRequiredService<ObjectPoolProvider>().Create(new StringBuilderPooledObjectPolicy()));

            services
                .AddTeamCloudAdapterFramework()
                .AddTeamCloudAdapter<AzureResourceManagerAdapter>()
                .AddTeamCloudAdapter<AzureDevOpsAdapter>()
                .AddTeamCloudAdapter<GitHubAdapter>();

            services
                .AddSingleton<IDocumentExpanderProvider>(serviceProvider => new DocumentExpanderProvider(serviceProvider))
                .AddSingleton<IDocumentExpander, ProjectIdentityExpander>()
                .AddSingleton<IDocumentExpander, DeploymentScopeExpander>()
                .AddSingleton<IDocumentExpander, ComponentTaskExpander>()
                .AddSingleton<IDocumentExpander, ComponentExpander>()
                .AddSingleton<IDocumentExpander, UserExpander>();

            ConfigureAuthentication(services);
            ConfigureAuthorization(services);

            services
                .AddApplicationInsightsTelemetry()
                .AddMvc();

            services
                .AddRouting(options =>
                {
                    options.ConstraintMap.Add("userId", typeof(UserIdentifierRouteConstraint));
                    options.ConstraintMap.Add("organizationId", typeof(OrganizationIdentifierRouteConstraint));
                    options.ConstraintMap.Add("projectId", typeof(ProjectIdentifierRouteConstraint));
                    options.ConstraintMap.Add("componentId", typeof(ComponentIdentifierRouteConstraint));
                    options.ConstraintMap.Add("deploymentScopeId", typeof(DeploymentScopeIdentifierConstraint));
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
                            Url = new Uri("https://github.com/microsoft/TeamCloud/blob/main/LICENSE")
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
                            var tenantId = context.Principal.GetTenantId();

                            var userClaims = await context.HttpContext.ResolveClaimsAsync(tenantId, userId).ConfigureAwait(false);
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
