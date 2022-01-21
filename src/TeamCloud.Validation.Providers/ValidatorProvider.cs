/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TeamCloud.Validation.Providers;

public sealed class ValidatorProvider : IValidatorProvider, IValidatorProviderConfig
{
    private readonly ReaderWriterLockSlim assemblyCacheLock = new ReaderWriterLockSlim();
    private readonly ConcurrentDictionary<Assembly, IEnumerable<Type>> assemblyCache = new ConcurrentDictionary<Assembly, IEnumerable<Type>>();
    private readonly ConcurrentDictionary<Type, IEnumerable<Type>> validatorMap = new ConcurrentDictionary<Type, IEnumerable<Type>>();

    private readonly IServiceProvider serviceProvider;

    internal ValidatorProvider(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IEnumerable<IValidator> GetValidators<T>()
        => GetValidators(typeof(T));

    public IEnumerable<IValidator> GetValidators(Type typeToValidate)
    {
        if (typeToValidate is null)
        {
            throw new ArgumentNullException(nameof(typeToValidate));
        }

        if (!assemblyCache.ContainsKey(typeToValidate.Assembly))
        {
            Register(typeToValidate.Assembly);
        }

        foreach (var validatorType in GetValidatorTypes(typeToValidate))
        {
            var validator = ActivatorUtilities
                .GetServiceOrCreateInstance(serviceProvider, validatorType) as IValidator;

            if (validator?.CanValidateInstancesOfType(typeToValidate) ?? false)
                yield return validator;
        }
    }

    private IEnumerable<Type> GetValidatorTypes(Type typeToValidate)
    {
        var resolvedValidatorTypes = Enumerable.Empty<Type>();

        if (typeToValidate is not null)
        {
            var exitLock = false;

            if (!assemblyCacheLock.IsReadLockHeld)
            {
                assemblyCacheLock.EnterReadLock();
                exitLock = true; // we enter it - we exit it
            }

            try
            {
                resolvedValidatorTypes = validatorMap.GetOrAdd(typeToValidate, _ =>
                {
                    var validatorTyped = typeof(IValidator<>).MakeGenericType(typeToValidate);

                    var validatorTypes = new HashSet<Type>(assemblyCache.Values
                        .SelectMany(value => value)
                        .Where(t => validatorTyped.IsAssignableFrom(t)));

                    foreach (var interfaceValidatorType in typeToValidate.GetInterfaces().SelectMany(i => GetValidatorTypes(i)))
                        validatorTypes.Add(interfaceValidatorType);

                    foreach (var baseValidatorType in GetValidatorTypes(typeToValidate.BaseType))
                        validatorTypes.Add(baseValidatorType);

                    return validatorTypes;
                });
            }
            finally
            {
                if (exitLock) assemblyCacheLock.ExitReadLock();
            }
        }

        return resolvedValidatorTypes;
    }

    public IValidatorProviderConfig Register(Assembly assembly)
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        assemblyCacheLock.EnterWriteLock();

        try
        {
            if (assemblyCache.TryAdd(assembly, ResolveValidatorTypes()))
                validatorMap.Clear();

            IEnumerable<Type> ResolveValidatorTypes()
                => assembly.GetTypes().Where(t => t.IsClass && typeof(IValidator).IsAssignableFrom(t));
        }
        finally
        {
            assemblyCacheLock.ExitWriteLock();
        }

        return this;
    }
}
