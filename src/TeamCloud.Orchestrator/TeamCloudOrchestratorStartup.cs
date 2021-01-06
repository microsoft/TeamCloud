/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamCloud.Audit;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Deployment.Providers;
using TeamCloud.Azure.Directory;
using TeamCloud.Azure.Resources;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Data;
using TeamCloud.Data.CosmosDb;
using TeamCloud.Git.Caching;
using TeamCloud.Git.Services;
using TeamCloud.Http;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator;
using TeamCloud.Orchestrator.Handlers;

[assembly: FunctionsStartup(typeof(TeamCloudOrchestratorStartup))]
[assembly: FunctionsImport(typeof(TeamCloudOrchestrationStartup))]
[assembly: FunctionsImport(typeof(TeamCloudOrchestrationDeploymentStartup))]

namespace TeamCloud.Orchestrator
{
    public class TeamCloudOrchestratorStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            var configuration = GetConfiguration(builder.Services);

            builder.Services
                .AddSingleton(configuration)
                .AddTeamCloudOptions(Assembly.GetExecutingAssembly())
                .AddTeamCloudOptionsShared()
                .AddTeamCloudHttp()
                .AddTeamCloudAudit()
                .AddMvcCore()
                .AddNewtonsoftJson();

            if (string.IsNullOrEmpty(configuration.GetValue<string>("Cache:Configuration")))
            {
                builder.Services
                    .AddDistributedMemoryCache()
                    .AddSingleton<IRepositoryCache, RepositoryCache>();
            }
            else
            {
                builder.Services
                    .AddDistributedRedisCache(options => configuration.Bind("Cache", options))
                    .AddSingleton<IRepositoryCache, RepositoryCache>();
            }

            builder.Services
                .AddSingleton<IOrganizationRepository, CosmosDbOrganizationRepository>()
                .AddSingleton<IUserRepository, CosmosDbUserRepository>()
                .AddSingleton<IDeploymentScopeRepository, CosmosDbDeploymentScopeRepository>()
                .AddSingleton<IProjectTemplateRepository, CosmosDbProjectTemplateRepository>()
                .AddSingleton<IComponentTemplateRepository, CosmosDbComponentTemplateRepository>()
                .AddSingleton<IComponentTaskRepository, CosmosDbComponentTaskRepository>()
                .AddSingleton<IComponentDeploymentRepository, CosmosDbComponentDeploymentRepository>()
                .AddSingleton<IProjectRepository, CosmosDbProjectRepository>()
                .AddSingleton<IComponentRepository, CosmosDbComponentRepository>()
                // .AddSingleton<IProjectLinkRepository, CosmosDbProjectLinkRepository>()
                .AddSingleton<IRepositoryService, RepositoryService>();

            // CAUTION - don't register an orchstrator command handler with the generic
            // ICommandHandler<> interface. purpose of this interface is the
            // command specific implementation logic. to register and identifiy a command
            // handler use the non-generic ICommandHandler interface.

            builder.Services
                .AddScoped<ICommandHandler, CommandOrchestrationHandler>()
                .AddScoped<ICommandHandler, ComponentCommandHandler>()
                .AddScoped<ICommandHandler, DeploymentScopeCommandHandler>()
                .AddScoped<ICommandHandler, OrganizationCommandHandler>()
                .AddScoped<ICommandHandler, OrganizationUserCommandHandler>()
                .AddScoped<ICommandHandler, ProjectCommandHandler>()
                // .AddScoped<ICommandHandler, ProjectLinkCommandHandler>()
                .AddScoped<ICommandHandler, ProjectTemplateCommandHandler>()
                .AddScoped<ICommandHandler, ProjectUserCommandHandler>();

            builder.Services
                .AddTeamCloudAzure(configuration =>
                {
                    configuration
                        .AddResources()
                        .AddDirectory()
                        .AddDeployment()
                        .SetDeploymentArtifactsProvider<AzureStorageArtifactsProvider>();
                });
        }

        private static IConfiguration GetConfiguration(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            return ConfigureEnvironment(environment, configuration).Build();
        }

        private static IConfigurationBuilder ConfigureEnvironment(IHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration);

            configuration = configurationBuilder
                .AddConfigurationService()
                .Build(); // refresh configuration root to get configuration service settings

            var keyVaultName = configuration["KeyVaultName"];

            if (!string.IsNullOrEmpty(keyVaultName))
            {
                // we use the managed identity of the service to authenticate at the KeyVault
                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                using var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                configurationBuilder.AddAzureKeyVault($"https://{keyVaultName}.vault.azure.net/", keyVaultClient, new DefaultKeyVaultSecretManager());
            }
            else if (hostingEnvironment.IsDevelopment())
            {
                // for development we use the local secret store as a fallback if not KeyVaultName is provided
                // see: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1

                try
                {
                    configurationBuilder.AddUserSecrets<TeamCloudOrchestratorStartup>();
                }
                catch (InvalidOperationException exc) when (exc.Message.Contains(nameof(UserSecretsIdAttribute), StringComparison.Ordinal))
                {
                    // swallow this exception and resume without user secrets
                }
            }

            return configurationBuilder;
        }

    }
}
