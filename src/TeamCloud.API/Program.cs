/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using TeamCloud.Configuration;

namespace TeamCloud.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configurationBuilder) => ConfigureEnvironment(hostingContext.HostingEnvironment, configurationBuilder))
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());

        private static void ConfigureEnvironment(IHostEnvironment hostingEnvironment, IConfigurationBuilder configurationBuilder)
        {
            var configurationRoot = configurationBuilder.Build();
            var configurationService = configurationRoot.GetConnectionString("ConfigurationService");

            if (!string.IsNullOrEmpty(configurationService))
            {
                // the configuration service connection string can either be an Azure App Configuration
                // service connection string or a file uri that points to a local settings file.                

                configurationRoot = configurationBuilder
                    .AddConfigurationService(configurationService, true)
                    .Build(); // refresh configuration root to get configuration service settings
            }

            var keyVaultName = configurationRoot["KeyVaultName"];

            if (!string.IsNullOrEmpty(keyVaultName))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope

                // we use the managed identity of the service to authenticate at the KeyVault

                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                var keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(
                        azureServiceTokenProvider.KeyVaultTokenCallback));

                //configurationBuilder.AddAzureKeyVault(
                //    $"https://{keyVaultName}.vault.azure.net/",
                //    keyVaultClient,
                //    new DefaultKeyVaultSecretManager());

#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            else if (hostingEnvironment.IsDevelopment())
            {
                // for development we use the local secret store as a fallback if not KeyVaultName is provided 
                // see: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1

                configurationBuilder.AddUserSecrets<Startup>();
            }
        }
    }
}
