/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TeamCloud.Configuration
{
    public static class Extensions
    {
        public static bool TryGetSection(this IConfiguration configuration, string key, out IConfigurationSection section)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            try
            {
                section = configuration.GetSection(key);
            }
            catch
            {
                section = null;
            }

            return (section != null);
        }

        public static bool TryBind<TOptions>(this IConfiguration configuration, string key, out TOptions options)
            where TOptions : class, new()
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            try
            {
                options = Activator.CreateInstance<TOptions>();

                configuration.GetSection(key).Bind(options);
            }
            catch
            {
                options = null;
            }

            return (options != null);
        }

        private static readonly MethodInfo AddOptionsMethod = typeof(Extensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .SingleOrDefault(mi => mi.Name.StartsWith(nameof(AddOptions), StringComparison.Ordinal) && mi.IsGenericMethodDefinition);

        private static readonly MethodInfo AddProxyMethod = typeof(Extensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .SingleOrDefault(mi => mi.Name.StartsWith(nameof(AddProxy), StringComparison.Ordinal) && mi.IsGenericMethodDefinition);

        public static IConfigurationBuilder AddConfigurationService(this IConfigurationBuilder configurationBuilder, string connectionString = null)
        {
            if (configurationBuilder is null)
                throw new ArgumentNullException(nameof(configurationBuilder));

            connectionString ??= configurationBuilder.Build()
                .GetConnectionString("ConfigurationService");

            if (string.IsNullOrWhiteSpace(connectionString))
                return configurationBuilder;

            var connectionStringExpanded = Regex.Replace(Environment.ExpandEnvironmentVariables(connectionString), "%CWD%", Environment.CurrentDirectory.Replace('\\', '/'));

            if (Uri.IsWellFormedUriString(connectionStringExpanded, UriKind.Absolute))
            {
                var configurationServiceFile = new Uri(connectionStringExpanded, UriKind.Absolute);

                if (!configurationServiceFile.IsFile)
                    throw new NotSupportedException($"Scheme '{configurationServiceFile.Scheme}' is not supported - use file:// instead");

                if (!File.Exists(configurationServiceFile.LocalPath))
                    Debug.WriteLine($"CAUTION !!! Configuration file {configurationServiceFile.LocalPath} could not be found.");

                return configurationBuilder.AddJsonFile(configurationServiceFile.LocalPath, false, true);
            }
            else
            {
                return configurationBuilder.AddAzureAppConfiguration(connectionString, false);
            }
        }

        public static IServiceCollection AddTeamCloudOptions(this IServiceCollection services, params Assembly[] additionalAssemblies)
        {
            var assemblies = new HashSet<Assembly>(additionalAssemblies)
            {
                Assembly.GetCallingAssembly()
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
            if (attribute is null)
                throw new ArgumentNullException(nameof(attribute));

            if (services is null)
                throw new ArgumentNullException(nameof(services));

            foreach (var contract in typeof(T).GetInterfaces().Concat(new Type[] { typeof(T) }))
            {
                services.AddTransient(contract, provider => ActivatorUtilities.CreateInstance<T>(provider));
            }
        }

        private static void AddOptions<T>(OptionsAttribute attribute, IServiceCollection services)
             where T : class, new()
        {
            if (attribute is null)
                throw new ArgumentNullException(nameof(attribute));

            if (services is null)
                throw new ArgumentNullException(nameof(services));

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
