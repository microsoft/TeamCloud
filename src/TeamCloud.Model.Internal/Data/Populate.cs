/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TeamCloud.Model.Data
{
    public interface IPopulate<T>
        where T : class, new()
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> PropertyCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        void PopulateFromExternalModel(T source)
        {
            var inerfaces = GetType().GetInterfaces().Intersect(GetType().GetInterfaces());

            var sourceProperties = PropertyCache.GetOrAdd(typeof(T), t => t.GetProperties());
            var targetProperties = PropertyCache.GetOrAdd(GetType(), t => t.GetProperties());

            foreach (var sourceProperty in sourceProperties)
            {
                var targetProperty = targetProperties.SingleOrDefault(p => p.Name == sourceProperty.Name);

                if (targetProperty?.PropertyType == sourceProperty.PropertyType)
                {
                    var sourceValue = sourceProperty.GetValue(source);

                    targetProperty.SetValue(this, sourceValue);

                    continue;
                }

                if (!(targetProperty is null) && !(sourceProperty.GetValue(source) is null))
                {
                    if (typeof(IEnumerable).IsAssignableFrom(sourceProperty.PropertyType) && typeof(IEnumerable).IsAssignableFrom(targetProperty.PropertyType))
                    {
                        var targetType = targetProperty.PropertyType
                            .GetInterfaces()
                            .FirstOrDefault(i => i.IsGenericType && typeof(IEnumerable).IsAssignableFrom(i))
                            .GetGenericArguments()
                            .SingleOrDefault();

                        var sourceType = sourceProperty.PropertyType
                            .GetInterfaces()
                            .FirstOrDefault(i => i.IsGenericType && typeof(IEnumerable).IsAssignableFrom(i))
                            .GetGenericArguments()
                            .SingleOrDefault();

                        var populateType = typeof(IPopulate<>).MakeGenericType(sourceType);

                        var populateMethod = populateType.GetMethod(nameof(PopulateFromExternalModel));

                        if (populateType.IsAssignableFrom(targetType))
                        {
                            var sourceEnumeration = (IEnumerable)sourceProperty.GetValue(source);

                            var targetItems = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));

                            foreach (var sourceItem in sourceEnumeration)
                            {
                                var targetItem = Activator.CreateInstance(targetType);

                                populateMethod.Invoke(targetItem, new[] { sourceItem });

                                targetItems.Add(targetItem);
                            }

                            targetProperty.SetValue(this, targetItems);
                        }
                    }
                    else
                    {
                        var populateInterfaceType = typeof(IPopulate<>).MakeGenericType(sourceProperty.PropertyType);

                        var populateInterfaceMethod = populateInterfaceType.GetMethod(nameof(PopulateFromExternalModel));

                        if (populateInterfaceType.IsAssignableFrom(targetProperty.PropertyType))
                        {
                            var sourceValue = sourceProperty.GetValue(source);

                            var targetValue = targetProperty.GetValue(this) ?? Activator.CreateInstance(targetProperty.PropertyType);

                            populateInterfaceMethod.Invoke(targetValue, new[] { sourceValue });

                            targetProperty.SetValue(this, targetValue);
                        }
                    }
                }
            }
        }

        T PopulateExternalModel(T target = null)
        {
            target ??= Activator.CreateInstance<T>();

            var inerfaces = typeof(T).GetInterfaces().Intersect(GetType().GetInterfaces());

            var sourceProperties = PropertyCache.GetOrAdd(GetType(), t => t.GetProperties());
            var targetProperties = PropertyCache.GetOrAdd(typeof(T), t => t.GetProperties());

            foreach (var sourceProperty in sourceProperties)
            {
                var targetProperty = targetProperties.SingleOrDefault(p => p.Name == sourceProperty.Name);

                if (targetProperty?.PropertyType == sourceProperty.PropertyType)
                {
                    var sourceValue = sourceProperty.GetValue(this);

                    targetProperty.SetValue(target, sourceValue);

                    continue;
                }

                if (!(targetProperty is null) && !(sourceProperty.GetValue(this) is null))
                {
                    if (typeof(IEnumerable).IsAssignableFrom(sourceProperty.PropertyType) && typeof(IEnumerable).IsAssignableFrom(targetProperty.PropertyType))
                    {
                        var targetType = targetProperty.PropertyType
                            .GetInterfaces()
                            .FirstOrDefault(i => i.IsGenericType && typeof(IEnumerable).IsAssignableFrom(i))
                            .GetGenericArguments()
                            .SingleOrDefault();

                        var sourceType = sourceProperty.PropertyType
                            .GetInterfaces()
                            .FirstOrDefault(i => i.IsGenericType && typeof(IEnumerable).IsAssignableFrom(i))
                            .GetGenericArguments()
                            .SingleOrDefault();

                        var populateType = typeof(IPopulate<>).MakeGenericType(targetType);

                        var populateMethod = populateType.GetMethod(nameof(PopulateExternalModel));

                        if (populateType.IsAssignableFrom(sourceType))
                        {
                            var sourceEnumeration = (IEnumerable)sourceProperty.GetValue(this);

                            var targetItems = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));

                            foreach (var sourceItem in sourceEnumeration)
                            {
                                var targetItem = Activator.CreateInstance(targetType);

                                populateMethod.Invoke(sourceItem, new[] { targetItem });

                                targetItems.Add(targetItem);
                            }

                            targetProperty.SetValue(target, targetItems);
                        }
                    }
                    else
                    {
                        var populateInterfaceType = typeof(IPopulate<>).MakeGenericType(targetProperty.PropertyType);

                        var populateInterfaceMethod = populateInterfaceType.GetMethod(nameof(PopulateExternalModel));

                        if (populateInterfaceType.IsAssignableFrom(sourceProperty.PropertyType))
                        {
                            var sourceValue = sourceProperty.GetValue(this);

                            var targetValue = targetProperty.GetValue(target) ?? Activator.CreateInstance(targetProperty.PropertyType);

                            populateInterfaceMethod.Invoke(sourceValue, new[] { targetValue });

                            targetProperty.SetValue(target, targetValue);
                        }
                    }
                }
            }

            return target;
        }
    }
}
