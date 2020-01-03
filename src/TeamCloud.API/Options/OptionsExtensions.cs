/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TeamCloud.API
{
    public static class OptionsExtensions
    {
        private static readonly MethodInfo AddOptionsMethod = typeof(OptionsExtensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .SingleOrDefault(mi => mi.Name.StartsWith(nameof(AddOptions)) && mi.IsGenericMethodDefinition);

        public static void AddOptions(this IServiceCollection services, Assembly assembly)
        {
            var optionsMap = assembly.GetTypes()
                .Where(type => type.IsClass && type.IsDefined(typeof(OptionsAttribute)))
                .ToDictionary(type => type, type => type.GetCustomAttribute<OptionsAttribute>());

            foreach (var kvp in optionsMap)
            {
                MethodInfo method = AddOptionsMethod.MakeGenericMethod(kvp.Key);

                method.Invoke(null, new object[] { kvp.Value, services });
            }
        }

        private static void AddOptions<T>(OptionsAttribute attribute, IServiceCollection services)
             where T : class, new()
        {
            services.AddOptions<T>()
                .Configure<IConfiguration>((options, config) =>
                {
                    if (!attribute.IsConfigRoot)
                    {
                        config = string.IsNullOrEmpty(attribute.SectionName)
                            ? config.GetSection(typeof(T).Name)
                            : config.GetSection(attribute.SectionName);
                    }

                    config.Bind(options);
                });

            services.AddTransient<T>((provider) => provider.GetRequiredService<IOptions<T>>().Value);

            foreach (var contract in typeof(T).GetInterfaces())
            {
                services.AddTransient(contract, (provider) => provider.GetRequiredService<IOptions<T>>().Value);
            }
        }
    }
}
