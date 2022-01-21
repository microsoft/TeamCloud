/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Validation.Providers;

public static class ValidatorExtensions
{
    public static IServiceCollection AddTeamCloudValidationProvider(this IServiceCollection services, Action<IValidatorProviderConfig> configure = null)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        services.TryAddSingleton<IValidatorProvider>(serviceProvider =>
        {
            var validatorProvider = new ValidatorProvider(serviceProvider);

            if (configure is not null && validatorProvider is IValidatorProviderConfig validatorProviderConfig)
            {
                configure(validatorProviderConfig);
            }

            return validatorProvider;
        });

        return services;
    }

}
