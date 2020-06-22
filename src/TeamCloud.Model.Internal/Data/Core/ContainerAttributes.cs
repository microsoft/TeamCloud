/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Internal.Data.Core
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class PartitionKeyAttribute : Attribute
    {
        public static string GetPath<T>(bool camelCase = false)
            where T : class
        {
            var name = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<PartitionKeyAttribute>() != null)
                .SingleOrDefault()?.Name;

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
            where T : class
        {
            var resolver = new CamelCasePropertyNamesContractResolver();

            return typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<UniqueKeyAttribute>() != null)
                .Select(p => $"/{(camelCase ? resolver.GetResolvedPropertyName(p.Name) : p.Name)}");
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DatabaseIgnoreAttribute : Attribute
    { }

}
