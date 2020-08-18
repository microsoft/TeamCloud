/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TeamCloud.Model.Validation
{

    public class ValidatorFactory : IValidatorFactory
    {
        private static readonly Lazy<ValidatorFactory> defaultFactoryInstance = new Lazy<ValidatorFactory>(new ValidatorFactory(Assembly.GetExecutingAssembly()));

        public static IValidatorFactory DefaultFactory { get; } = defaultFactoryInstance.Value;

        private readonly ConcurrentDictionary<Type, Type[]> validatorMap = new ConcurrentDictionary<Type, Type[]>();

        private static IEnumerable<Type> GetValidatorTargetTypes(Type validatorType)
        {
            if (!typeof(IValidator).IsAssignableFrom(validatorType))
                return Enumerable.Empty<Type>();

            var validatorTargetTypes = validatorType.GetInterfaces()
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(type => type.GetGenericArguments().First());

            if (!validatorTargetTypes.Any())
                validatorTargetTypes = Enumerable.Repeat(typeof(object), 1);

            return validatorTargetTypes;
        }

        private ValidatorFactory(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            this.AddValidators(assembly);
        }

        public void AddValidator<T>(bool merge = true)
            where T : class, IValidator
        {

        }

        public void AddValidators(Assembly assembly, bool merge = true)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            var validatorMapByAssembly = assembly.GetTypes()
                .Where(type => type.IsClass && typeof(IValidator).IsAssignableFrom(type))
                .SelectMany(validatorType => GetValidatorTargetTypes(validatorType).Select(targetType => new KeyValuePair<Type, Type>(targetType, validatorType)))
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(grp => grp.Key, grp => grp.Select(item => item.Value).ToArray());

            foreach (var targetType in validatorMapByAssembly.Keys)
            {
                if (merge && validatorMap.TryGetValue(targetType, out var validatorTypes))
                {
                    validatorMap[targetType] = validatorTypes
                        .Concat(validatorMapByAssembly[targetType])
                        .Distinct()
                        .ToArray();
                }
                else
                {
                    validatorMap[targetType] = validatorMapByAssembly[targetType];
                }
            }
        }

        public IEnumerable<Type> GetValidatorTypes<T>()
            => GetValidatorTypes(typeof(T));

        public IEnumerable<Type> GetValidatorTypes(Type typeToValidate)
        {
            if (typeToValidate is null)
                throw new ArgumentNullException(nameof(typeToValidate));

            if (!typeToValidate.IsClass && !typeToValidate.IsInterface)
                return Enumerable.Empty<Type>();

            var validatorTypes = new List<Type>();

            if (validatorMap.TryGetValue(typeToValidate, out var validatorTypesByClass))
                validatorTypes.AddRange(validatorTypesByClass);

            foreach (var validationTargetInterfaceType in typeToValidate.GetInterfaces())
            {
                if (validatorMap.TryGetValue(validationTargetInterfaceType, out var validatorTypesByInterface))
                    validatorTypes.AddRange(validatorTypesByInterface);
            }

            if (typeToValidate.BaseType != null)
                validatorTypes.AddRange(GetValidatorTypes(typeToValidate.BaseType));

            return validatorTypes.Distinct();
        }

        public IEnumerable<IValidator> GetValidators<T>(IServiceProvider serviceProvider = null)
            => GetValidators(typeof(T), serviceProvider);

        public IEnumerable<IValidator> GetValidators(Type typeToValidate, IServiceProvider serviceProvider = null)
        {
            if (typeToValidate is null)
                throw new ArgumentNullException(nameof(typeToValidate));

            if (!typeToValidate.IsClass && !typeToValidate.IsInterface)
                yield break;

            foreach (var validatorType in GetValidatorTypes(typeToValidate))
            {
                var validatorInstance = (IValidator)(serviceProvider is null
                    ? Activator.CreateInstance(validatorType)
                    : ActivatorUtilities.CreateInstance(serviceProvider, validatorType));

                if (validatorInstance.CanValidateInstancesOfType(typeToValidate))
                    yield return validatorInstance;
            }
        }
    }
}
