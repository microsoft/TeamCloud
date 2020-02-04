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
using Microsoft.WindowsAzure.Storage;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployments.Providers;
using TeamCloud.Configuration;
using TeamCloud.Data;
using TeamCloud.Data.CosmosDb;
using TeamCloud.Orchestrator;
using TeamCloud.Orchestrator.Providers;

[assembly: FunctionsStartup(typeof(Startup))]

namespace TeamCloud.Orchestrator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services
                .AddSingleton(GetConfiguration(builder.Services))
                .AddOptions(Assembly.GetExecutingAssembly())
                .AddMvcCore()
                .AddNewtonsoftJson();

            builder.Services
                .AddScoped<IProjectsRepository, CosmosDbProjectsRepository>()
                .AddScoped<ITeamCloudRepository, CosmosDbTeamCloudRepository>()
                .AddScoped<IProjectTypesRepository, CosmosDbProjectTypesRepository>()
                .AddAzure(configuration =>
                {
                    var provider = builder.Services.BuildServiceProvider();
                    var options = provider.GetRequiredService<IAzureStorageArtifactsOptions>();

                    if (CloudStorageAccount.TryParse(options.ConnectionString, out var storageAccount) && storageAccount.IsDevelopmentStorage())
                    {
                        // if our artifact storage provider points towards a development storage
                        // account (emulator) we need to set a token provider. this way an arm
                        // deployment can resolve a linked template (via ngrok) from the function app.

                        configuration.SetDeploymentTokenProvider<AzureDeploymentTokenProvider>();
                    }

                    configuration.SetDeploymentArtifactsProvider<AzureStorageArtifactsProvider>();
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

                using (var keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(
                        azureServiceTokenProvider.KeyVaultTokenCallback)))
                {
                    configurationBuilder.AddAzureKeyVault(
                        $"https://{keyVaultName}.vault.azure.net/",
                        keyVaultClient,
                        new DefaultKeyVaultSecretManager());
                }
            }
            else if (hostingEnvironment.IsDevelopment())
            {
                // for development we use the local secret store as a fallback if not KeyVaultName is provided
                // see: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1

                try
                {
                    configurationBuilder.AddUserSecrets<Startup>();
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
