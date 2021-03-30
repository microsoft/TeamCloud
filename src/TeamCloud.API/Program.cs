/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TeamCloud.Configuration;

namespace TeamCloud.API
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = await CreateHostBuilder(args)
                .Build()
                .InitializeAsync()
                .ConfigureAwait(false);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configurationBuilder) => ConfigureEnvironment(hostingContext.HostingEnvironment, configurationBuilder))
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());

        private static void ConfigureEnvironment(IHostEnvironment hostingEnvironment, IConfigurationBuilder configurationBuilder)
        {
            var configurationRoot = configurationBuilder.Build();

            configurationRoot = configurationBuilder
                .AddConfigurationService()
                .Build(); // refresh configuration root to get configuration service settings

            var keyVaultName = configurationRoot["KeyVaultName"];

            if (!string.IsNullOrEmpty(keyVaultName) && Uri.TryCreate($"https://{keyVaultName}.vault.azure.net/", UriKind.Absolute, out var keyVaultUri))
            {
                // we use the managed identity of the service to authenticate at the KeyVault
                // var azureServiceTokenProvider = new AzureServiceTokenProvider();

                // using var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                // configurationBuilder.AddAzureKeyVault($"https://{keyVaultName}.vault.azure.net/", keyVaultClient, new DefaultKeyVaultSecretManager());
                var azureCredentialOptions = new DefaultAzureCredentialOptions();
                configurationBuilder.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential(azureCredentialOptions));
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
