/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Secrets
{
    public static class Extensions
    {
        public static IServiceCollection AddTeamCloudSecrets(this IServiceCollection services)
        {
            services
                .TryAddSingleton<ISecretsStoreProvider>(provider => new SecretsStoreProvider(provider));

            return services;
        }
    }
}
