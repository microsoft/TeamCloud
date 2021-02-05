/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ContainerNameAttribute : Attribute
    {
        public static string GetNameOrDefault<T>()
            => GetNameOrDefault(typeof(T));

        public static string GetNameOrDefault(Type containerType)
        {
            if (containerType is null)
                throw new ArgumentNullException(nameof(containerType));

            var attribute = containerType
                .GetCustomAttribute<ContainerNameAttribute>();

            return attribute?.Name
                ?? Regex.Replace(containerType.Name, "Document$", string.Empty, RegexOptions.None);
        }

        public ContainerNameAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not NULL, EMPTY, or WHITESPACE", nameof(name));

            Name = name.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class PartitionKeyAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo> PropertyCache
            = new ConcurrentDictionary<Type, PropertyInfo>();

        private static PropertyInfo GetProperty(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return PropertyCache.GetOrAdd(type, (key) =>
            {
                var property = type.GetProperties()
                    .Where(p => p.GetCustomAttribute<PartitionKeyAttribute>() != null)
                    .SingleOrDefault();

                if (property.PropertyType != typeof(string))
                    throw new NotSupportedException($"{typeof(PartitionKeyAttribute)} is only supported on properties with the type String");

                return property;
            });
        }

        public static object GetValue<T>(T obj)
            where T : class
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            return GetProperty(typeof(T)).GetValue(obj);
        }

        public static string GetPath<T>(bool camelCase = false)
            => GetPath(typeof(T), camelCase);

        public static string GetPath(Type type, bool camelCase = false)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var name = GetProperty(type)?.Name;

            if (string.IsNullOrEmpty(name))
                return name;

            if (camelCase)
                name = new CamelCasePropertyNamesContractResolver().GetResolvedPropertyName(name);

            return $"/{name}";
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class UniqueKeyAttribute : Attribute
    {
        public static IEnumerable<string> GetPaths<T>(bool camelCase = false)
            => GetPaths(typeof(T), camelCase);

        public static IEnumerable<string> GetPaths(Type type, bool camelCase = false)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var resolver = new CamelCasePropertyNamesContractResolver();

            return type.GetProperties()
                .Where(p => p.GetCustomAttribute<UniqueKeyAttribute>() != null)
                .Select(p => $"/{(camelCase ? resolver.GetResolvedPropertyName(p.Name) : p.Name)}");
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class DatabaseIgnoreAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SoftDeleteAttribute : Attribute
    {
        public static int? GetSoftDeleteTTL<T>()
            => GetSoftDeleteTTL(typeof(T));

        public static int? GetSoftDeleteTTL(Type containerType)
        {
            if (containerType is null)
                throw new ArgumentNullException(nameof(containerType));

            var attribute = containerType
                .GetCustomAttribute<SoftDeleteAttribute>();

            return attribute?.TTL;
        }

        public SoftDeleteAttribute(int ttl)
        {
            if (ttl <= 0)
                throw new ArgumentException("Value must be greater than 0", nameof(ttl));

            TTL = ttl;
        }

        public int TTL { get; }
    }
}
