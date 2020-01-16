/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TeamCloud.Configuration
{
    public static class Extensions
    {
        private static readonly MethodInfo AddOptionsMethod = typeof(Extensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .SingleOrDefault(mi => mi.Name.StartsWith(nameof(AddOptions)) && mi.IsGenericMethodDefinition);

        private static readonly MethodInfo AddProxyMethod = typeof(Extensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .SingleOrDefault(mi => mi.Name.StartsWith(nameof(AddProxy)) && mi.IsGenericMethodDefinition);

        public static IConfigurationBuilder AddConfigurationService(this IConfigurationBuilder configurationBuilder, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                connectionString = configurationBuilder.Build().GetConnectionString("ConfigurationService");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Unable to add configuration service without connection string ('ConfigurationService')");

            var connectionStringExpanded = Environment.ExpandEnvironmentVariables(connectionString);

            if (Uri.IsWellFormedUriString(connectionStringExpanded, UriKind.Absolute))
            {
                var configurationServiceFile = new Uri(connectionStringExpanded, UriKind.Absolute);

                if (!configurationServiceFile.IsFile)
                    throw new NotSupportedException($"Scheme '{configurationServiceFile.Scheme}' is not supported - use file:// instead");

                return configurationBuilder.AddJsonFile(configurationServiceFile.LocalPath, false, true);
            }
            else
            {
                return configurationBuilder.AddAzureAppConfiguration(connectionString, false);
            }
        }

        public static IServiceCollection AddOptions(this IServiceCollection services, Assembly assembly, params Assembly[] additionalAssemblies)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var assemblies = new HashSet<Assembly>(additionalAssemblies)
            {
                assembly, Assembly.GetExecutingAssembly()
            };

            var optionsMap = assemblies
                .Where(asm => !asm.IsDynamic)
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsClass && type.IsDefined(typeof(OptionsAttribute)))
                .ToDictionary(type => type, type => type.GetCustomAttribute<OptionsAttribute>());

            foreach (var kvp in optionsMap)
            {
                if (!string.IsNullOrEmpty(kvp.Value.SectionName) && kvp.Key.GetConstructor(Type.EmptyTypes) is null)
                    throw new NotSupportedException($"Options with section name defined require a default constructor.");

                MethodInfo method = string.IsNullOrEmpty(kvp.Value.SectionName)
                    ? AddProxyMethod.MakeGenericMethod(kvp.Key)
                    : AddOptionsMethod.MakeGenericMethod(kvp.Key);

                method.Invoke(null, new object[] { kvp.Value, services });
            }

            return services;
        }

        private static void AddProxy<T>(OptionsAttribute attribute, IServiceCollection services)
            where T : class
        {
            foreach (var contract in typeof(T).GetInterfaces().Concat(new Type[] { typeof(T) }))
            {
                services.AddTransient(contract, provider => ActivatorUtilities.CreateInstance<T>(provider));
            }
        }

        private static void AddOptions<T>(OptionsAttribute attribute, IServiceCollection services)
             where T : class, new()
        {
            services
                .AddOptions<T>()
                .Configure<IConfiguration>((options, config) => config.GetSection(attribute.SectionName)?.Bind(options));

            foreach (var contract in typeof(T).GetInterfaces().Concat(new Type[] { typeof(T) }))
            {
                services.AddTransient(contract, ResolveOptions);
            }

            static T ResolveOptions(IServiceProvider provider)
            {
                try
                {
                    return provider.GetRequiredService<IOptionsSnapshot<T>>().Value;
                }
                catch (InvalidOperationException)
                {
                    return provider.GetRequiredService<IOptionsMonitor<T>>().CurrentValue;
                }
            }
        }
    }
}
