/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Reflection;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamCloud.Adapters;
using TeamCloud.Adapters.AzureDevOps;
using TeamCloud.Adapters.AzureResourceManager;
using TeamCloud.Adapters.GitHub;
using TeamCloud.Adapters.Kubernetes;
using TeamCloud.Audit;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Deployment.Providers;
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
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model.Validation;
using TeamCloud.Notification.Smtp;
using TeamCloud.Orchestration;
using TeamCloud.Orchestration.Deployment;
using TeamCloud.Orchestrator;
using TeamCloud.Orchestrator.Command;
using TeamCloud.Orchestrator.Command.Data;
using TeamCloud.Orchestrator.Options;
using TeamCloud.Secrets;
using TeamCloud.Serialization.Encryption;
using TeamCloud.Validation.Providers;

[assembly: FunctionsStartup(typeof(TeamCloudOrchestratorStartup))]
[assembly: FunctionsImport(typeof(TeamCloudOrchestrationStartup))]
[assembly: FunctionsImport(typeof(TeamCloudOrchestrationDeploymentStartup))]

namespace TeamCloud.Orchestrator;

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
            .AddTeamCloudHttp()
            .AddTeamCloudAudit()
            .AddTeamCloudSecrets()
            .AddMvcCore()
            .AddNewtonsoftJson();

        builder.Services
            .AddTeamCloudAdapterProvider(configuration =>
            {
                configuration
                    .Register<AzureResourceManagerAdapter>()
                    .Register<AzureDevOpsAdapter>()
                    .Register<GitHubAdapter>()
                    .Register<KubernetesAdapter>();
            });

        var notificationSmtpOptions = builder.Services
            .BuildServiceProvider()
            .GetService<INotificationSmtpOptions>();

        if (!string.IsNullOrWhiteSpace(notificationSmtpOptions?.Host) &&
            !string.IsNullOrWhiteSpace(notificationSmtpOptions?.SenderAddress))
            builder.Services.AddTeamCloudNotificationSmtpSender(notificationSmtpOptions);

        var databaseOptions = builder.Services
            .BuildServiceProvider()
            .GetService<TeamCloudDatabaseOptions>();

        builder.Services
            .AddCosmosCache(options =>
            {
                options.ClientBuilder = new CosmosClientBuilder(databaseOptions.ConnectionString);
                options.DatabaseName = $"{databaseOptions.DatabaseName}Cache";
                options.ContainerName = "DistributedCache";
                options.CreateIfNotExists = true;
            })
            .AddSingleton<IRepositoryCache, RepositoryCache>();

        if (configuration.TryBind<EncryptionOptions>("Encryption", out var encryptionOptions) && !string.IsNullOrEmpty(encryptionOptions.KeyStorage))
        {
            const string EncryptionContainerName = "encryption";

            new BlobContainerClient(encryptionOptions.KeyStorage, EncryptionContainerName)
                .CreateIfNotExists();

            var dataProtectionBuilder = builder.Services
                .AddDataProtection()
                .SetApplicationName("TeamCloud")
                .PersistKeysToAzureBlobStorage(encryptionOptions.KeyStorage, EncryptionContainerName, "keys.xml");

            if (!string.IsNullOrEmpty(encryptionOptions.KeyVault))
            {
                //dataProtectionBuilder.ProtectKeysWithAzureKeyVault()
                throw new NotImplementedException();
            }
        }

        builder.Services
            .AddSingleton<IDocumentExpanderProvider>(serviceProvider => new DocumentExpanderProvider(serviceProvider))
            .AddSingleton<IDocumentExpander, DeploymentScopeExpander>()
            .AddSingleton<IDocumentExpander, ProjectExpander>()
            .AddSingleton<IDocumentExpander, ProjectIdentityExpander>()
            .AddSingleton<IDocumentExpander, ComponentTaskExpander>()
            .AddSingleton<IDocumentExpander, ComponentExpander>()
            .AddSingleton<IDocumentExpander, UserExpander>();

        builder.Services
            .AddSingleton<IDocumentSubscriptionProvider>(serviceProvider => new DocumentSubscriptionProvider(serviceProvider))
            .AddSingleton<IDocumentSubscription, DocumentNotificationSubscription>();

        builder.Services
            .AddSingleton<IOrganizationRepository, CosmosDbOrganizationRepository>()
            .AddSingleton<IUserRepository, CosmosDbUserRepository>()
            .AddSingleton<IDeploymentScopeRepository, CosmosDbDeploymentScopeRepository>()
            .AddSingleton<IProjectIdentityRepository, CosmosDbProjectIdentityRepository>()
            .AddSingleton<IProjectTemplateRepository, CosmosDbProjectTemplateRepository>()
            .AddSingleton<IComponentTemplateRepository, CosmosDbComponentTemplateRepository>()
            .AddSingleton<IComponentTaskRepository, CosmosDbComponentTaskRepository>()
            .AddSingleton<IProjectRepository, CosmosDbProjectRepository>()
            .AddSingleton<IComponentRepository, CosmosDbComponentRepository>()
            .AddSingleton<IScheduleRepository, CosmosDbScheduleRepository>()
            .AddSingleton<IRepositoryService, RepositoryService>();


        // CAUTION - don't register an orchstrator command handler with the generic
        // ICommandHandler<> interface. purpose of this interface is the
        // command specific implementation logic. to register and identifiy a command
        // handler use the non-generic ICommandHandler interface.

        var commandHandlerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommandHandler).IsAssignableFrom(t));

        foreach (var commandHandlerType in commandHandlerTypes)
            builder.Services.AddScoped(typeof(ICommandHandler), commandHandlerType);
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

            configurationBuilder.AddAzureKeyVault(new Uri($"https://{keyVaultName}.vault.azure.net/"), new DefaultAzureCredential());
        }
        else if (hostingEnvironment.IsDevelopment())
        {
            // for development we use the local secret store as a fallback if not KeyVaultName is provided
            // see: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1

            try
            {
                configurationBuilder.AddUserSecrets<TeamCloudOrchestratorStartup>(true);
            }
            catch (InvalidOperationException exc) when (exc.Message.Contains(nameof(UserSecretsIdAttribute), StringComparison.Ordinal))
            {
                // swallow this exception and resume without user secrets
            }
        }

        return configurationBuilder;
    }

}
