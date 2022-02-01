/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TeamCloud.Configuration;
using System;

namespace TeamCloud.API;

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

        if (!string.IsNullOrEmpty(keyVaultName))
        {
            // we use the managed identity of the service to authenticate at the KeyVault

            configurationBuilder.AddAzureKeyVault(new Uri($"https://{keyVaultName}.vault.azure.net/"), new DefaultAzureCredential());
        }
        else if (hostingEnvironment.IsDevelopment())
        {
            // for development we use the local secret store as a fallback if not KeyVaultName is provided
            // see: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1

            configurationBuilder.AddUserSecrets<Startup>(true);
        }
    }
}
