/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
            if (hostingEnvironment.IsDevelopment())
                configurationBuilder.AddUserSecrets<Startup>();

            var configurationRoot = configurationBuilder.Build();
            var configurationService = configurationRoot["ConnectionStrings:ConfigurationService"];

            if (!string.IsNullOrEmpty(configurationService))
            {
                configurationBuilder.AddAzureAppConfiguration(options =>
                {
                    options = options.Connect(configurationService);

                    var keyVaultName = configurationRoot["ConnectionStrings:KeyVaultName"];
                    var keyVaultUrl = $"https://{keyVaultName}.vault.azure.net/";

                    if (!string.IsNullOrEmpty(keyVaultName))
                    {
                        var azureServiceTokenProvider = new AzureServiceTokenProvider();

                        var keyVaultCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
                        var keyVaultClient = new KeyVaultClient(keyVaultCallback);

                        options.UseAzureKeyVault(keyVaultClient);
                    }
                });
            }
        }
    }
}
