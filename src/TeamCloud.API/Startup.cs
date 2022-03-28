/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Azure.Storage.Blobs;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Identity.Web;
using Microsoft.IO;
using Microsoft.OpenApi.Models;
using TeamCloud.Adapters;
using TeamCloud.Adapters.AzureDevOps;
using TeamCloud.Adapters.AzureResourceManager;
using TeamCloud.Adapters.GitHub;
using TeamCloud.Adapters.Kubernetes;
using TeamCloud.API.Auth;
using TeamCloud.API.Auth.Schemes;
using TeamCloud.API.Middleware;
using TeamCloud.API.Routing;
using TeamCloud.API.Services;
using TeamCloud.API.Swagger;
using TeamCloud.Audit;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Deployment.Providers;
using TeamCloud.Microsoft.Graph;
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
using TeamCloud.Model.Validation;
using TeamCloud.Secrets;
using TeamCloud.Serialization.Encryption;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API;

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
                    .SetPreflightMaxAge(TimeSpan.FromDays(1))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        }
        else
        {
            app
                .UseHsts();
            // Our app currently runs in a container in App Service
            // which handels https, so this is not needed and causes
            // errors when enabled
            // .UseHttpsRedirection();
        }

        app
            .UseSwagger()
            .UseSwaggerUI(setup =>
            {
                setup.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamCloud API v1");
                setup.OAuthClientId(resourceManagerOptions.ClientId);
                setup.OAuthClientSecret("");
                setup.OAuthUsePkce();
            });

        app
            .UseRouting()
            .UseAuthentication()
            .UseMiddleware<EnsureTeamCloudModelMiddleware>()
            .UseAuthorization()
            .UseEndpoints(endpoints => endpoints.MapControllers());

        EncryptedValueProvider.DefaultDataProtectionProvider = app.ApplicationServices.GetDataProtectionProvider();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTeamCloudOptions(Assembly.GetExecutingAssembly())
            .AddTeamCloudOptionsShared()
            .AddTeamCloudGraph()
            .AddTeamCloudAzure(configuration =>
            {
                configuration
                    .AddResources()
                    .AddDeployment()
                    .SetDeploymentArtifactsProvider<AzureStorageArtifactsProvider>();
            })
            .AddTeamCloudValidationProvider(configuration =>
            {
                configuration
                    .Register(Assembly.GetExecutingAssembly())
                    .RegisterModelValidators();
            })
            .AddTeamCloudAdapterProvider(configuration =>
            {
                configuration
                    .Register<AzureResourceManagerAdapter>()
                    .Register<AzureDevOpsAdapter>()
                    .Register<GitHubAdapter>()
                    .Register<KubernetesAdapter>();
            })
            .AddTeamCloudAudit()
            .AddTeamCloudHttp()
            .AddTeamCloudSecrets();

        if (Configuration.TryBind<AzureCosmosDbOptions>("Azure:CosmosDb", out var azureCosmosDbOptions))
        {
            services
                .AddCosmosCache(options =>
                {
                    options.ClientBuilder = new CosmosClientBuilder(azureCosmosDbOptions.ConnectionString);
                    options.DatabaseName = $"{azureCosmosDbOptions.DatabaseName}Cache";
                    options.ContainerName = "DistributedCache";
                    options.CreateIfNotExists = true;
                })
                .AddSingleton<IRepositoryCache, RepositoryCache>();
        }

        if (Configuration.TryBind<EncryptionOptions>("Encryption", out var encryptionOptions) && !string.IsNullOrEmpty(encryptionOptions.KeyStorage))
        {
            const string EncryptionContainerName = "encryption";

            new BlobContainerClient(encryptionOptions.KeyStorage, EncryptionContainerName)
                .CreateIfNotExists();

            var dataProtectionBuilder = services
                .AddDataProtection()
                .SetApplicationName("TeamCloud")
                .PersistKeysToAzureBlobStorage(encryptionOptions.KeyStorage, EncryptionContainerName, "keys.xml");

            if (!string.IsNullOrEmpty(encryptionOptions.KeyVault))
            {
                //dataProtectionBuilder.ProtectKeysWithAzureKeyVault()
                throw new NotImplementedException();
            }
        }

        services
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
            .AddSingleton<IOrganizationRepository, CosmosDbOrganizationRepository>()
            .AddSingleton<IUserRepository, CosmosDbUserRepository>()
            .AddSingleton<IDeploymentScopeRepository, CosmosDbDeploymentScopeRepository>()
            .AddSingleton<IProjectIdentityRepository, CosmosDbProjectIdentityRepository>()
            .AddSingleton<IProjectTemplateRepository, CosmosDbProjectTemplateRepository>()
            .AddSingleton<IComponentTemplateRepository, CosmosDbComponentTemplateRepository>()
            .AddSingleton<IProjectRepository, CosmosDbProjectRepository>()
            .AddSingleton<IComponentRepository, CosmosDbComponentRepository>()
            .AddSingleton<IComponentTaskRepository, CosmosDbComponentTaskRepository>()
            .AddSingleton<IScheduleRepository, CosmosDbScheduleRepository>()
            .AddSingleton<IClientErrorFactory, ClientErrorFactory>()
            .AddSingleton<OrchestratorService>()
            .AddSingleton<UserService>()
            .AddSingleton<OneTimeTokenService>()
            .AddSingleton<IRepositoryService, RepositoryService>()
            .AddSingleton<EnsureTeamCloudModelMiddleware>();


        services
            .AddSingleton<RecyclableMemoryStreamManager>()
            .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
            .AddSingleton(provider => provider.GetRequiredService<ObjectPoolProvider>().Create(new StringBuilderPooledObjectPolicy()));

        services
            .AddSingleton<IDocumentExpanderProvider>(serviceProvider => new DocumentExpanderProvider(serviceProvider))
            .AddSingleton<IDocumentExpander, ProjectIdentityExpander>()
            .AddSingleton<IDocumentExpander, DeploymentScopeExpander>()
            .AddSingleton<IDocumentExpander, ComponentTaskExpander>()
            .AddSingleton<IDocumentExpander, ComponentExpander>()
            .AddSingleton<IDocumentExpander, UserExpander>();


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
                options.ConstraintMap.Add("commandId", typeof(CommandIdentifierRouteConstraint));
                options.ConstraintMap.Add("taskId", typeof(TaskIdentifierRouteConstraint));
            })
            .AddControllers()
            .AddNewtonsoftJson()
            .ConfigureApiBehaviorOptions(options => options.SuppressMapClientErrors = true);

#pragma warning disable CA1308 // Normalize strings to uppercase

        ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, lambda) => memberInfo?.Name?.ToLowerInvariant();
        ValidatorOptions.Global.PropertyNameResolver = (type, memberInfo, lambda) => memberInfo?.Name?.ToLowerInvariant();

#pragma warning restore CA1308 // Normalize strings to uppercase

        if (Configuration.TryBind<AzureResourceManagerOptions>("Azure:ResourceManager", out var azureResourceManagerOptions))
        {
            ConfigureAuthentication(services, azureResourceManagerOptions);
            ConfigureAuthorization(services);
            ConfigureSwagger(services, azureResourceManagerOptions);
        }
        else
        {
            throw new ApplicationException("Failed to bind configuration section 'Azure:ResourceManager'");
        }
    }

#pragma warning restore CA1822 // Mark members as static

    private static void ConfigureSwagger(IServiceCollection services, AzureResourceManagerOptions azureResourceManagerOptions)
    {
        services
            .AddSwaggerGen(options =>
            {
                options.DocumentFilter<SwaggerDocumentFilter>();

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

                options.EnableAnnotations();
                options.UseInlineDefinitionsForEnums();

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"https://login.microsoftonline.com/{azureResourceManagerOptions.TenantId}/oauth2/v2.0/token"),
                            AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{azureResourceManagerOptions.TenantId}/oauth2/v2.0/authorize"),
                            Scopes = new Dictionary<string, string> {
                                    { "openid", "Sign you in" },
                                    { "http://TeamCloud.DEMO.Web/user_impersonation", "Access the TeamCloud API" }
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
                            new [] { "openid", "http://TeamCloud.DEMO.Web/user_impersonation" }
                        }
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
            })
            .AddSwaggerGenNewtonsoftSupport();
    }

    private static void ConfigureAuthentication(IServiceCollection services, AzureResourceManagerOptions azureResourceManagerOptions)
    {
        services
            .AddHttpContextAccessor()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddAdapterAuthentication()
            .AddMicrosoftIdentityWebApi(jwtOptions =>
            {
                // Disable audience validation
                jwtOptions.TokenValidationParameters.ValidateAudience = false;

                jwtOptions.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = async (TokenValidatedContext context) =>
                    {
                        var userId = context.Principal.GetObjectId();
                        var tenantId = context.Principal.GetTenantId();

                        var userClaims = await context.HttpContext.ResolveClaimsAsync(tenantId, userId).ConfigureAwait(false);
                        if (userClaims.Any()) context.Principal.AddIdentity(new ClaimsIdentity(userClaims));
                    }
                };
            }, identityOptions =>
            {
                identityOptions.ClientId = azureResourceManagerOptions.ClientId;
                identityOptions.ClientSecret = azureResourceManagerOptions.ClientSecret;
                identityOptions.TenantId = azureResourceManagerOptions.TenantId;
                identityOptions.Instance = "https://login.microsoftonline.com/";
            }, JwtBearerDefaults.AuthenticationScheme);
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services // Requires authentication across the API
            .AddMvc(options => options.Filters.Add(new AuthorizeFilter(AuthPolicies.Default)));

        services
            .AddTeamCloudAuthorization();
    }
}
