/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotLiquid;
using DotLiquid.NamingConventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Templates;

public static class TemplateExtensions
{
    static TemplateExtensions()
    {
        Template.NamingConvention = new CamelCaseNamingConvention();
        Template.RegisterFilter(typeof(TemplateFilters));
    }

    private static readonly HashSet<Type> RegisteredSafeTypes = new HashSet<Type>();

    public static string Merge(this string instance, object data)
    {
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));

        var templateHash = new Hash();

        if (data?.GetType() == typeof(string) || (data?.GetType().IsValueType ?? false))
        {
            templateHash = Hash.FromAnonymousObject(new { value = data });
        }
        else if (data?.GetType().IsClass ?? false)
        {
            RegisterSafeType(data.GetType());

            foreach (var member in GetMembers(data.GetType()))
                templateHash[GetMemberName(member)] = (member as PropertyInfo)?.GetValue(data) ?? (member as FieldInfo)?.GetValue(data);
        }

        return Template
            .Parse(instance)
            .Render(templateHash);
    }

    public static string GetManifestResourceTemplate(this Assembly instance, string resourceName, object data = null)
    {
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));

        var templateName = instance
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.Equals(resourceName, StringComparison.Ordinal));

        if (string.IsNullOrEmpty(templateName))
        {
            templateName = instance
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(templateName))
        {
            using var templateStream = instance.GetManifestResourceStream(templateName);
            using var templateReader = new StreamReader(templateStream);

            return templateReader.ReadToEnd().Merge(data);
        }

        return null;
    }

    private static void RegisterSafeType(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        else if (IsSupportedType(type))
        {
            var enumerableTypes = GetEnumerableTypes(type);

            if (enumerableTypes.Any())
            {
                foreach (var enumerableType in enumerableTypes)
                    RegisterSafeType(enumerableType);
            }
            else if (RegisteredSafeTypes.Add(type))
            {
                Template.RegisterSafeType(type, data => new ObjectProxy(data));

                var nestedTypes = GetMembers(type)
                    .Select(m => (m as PropertyInfo)?.PropertyType ?? (m as FieldInfo)?.FieldType);

                foreach (var nestedType in nestedTypes)
                    RegisterSafeType(nestedType);
            }
        }

        static bool IsSupportedType(Type type)
            => !type.IsValueType && type != typeof(object) && type != typeof(string);

        static IEnumerable<Type> GetEnumerableTypes(Type type)
            => type.GetInterfaces().ToArray().Prepend(type)
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .SelectMany(i => i.GetGenericArguments())
            .Distinct();
    }


    private static readonly ConcurrentDictionary<Type, MemberInfo[]> MemberCache = new ConcurrentDictionary<Type, MemberInfo[]>();

    private static IEnumerable<MemberInfo> GetMembers(Type type) => MemberCache.GetOrAdd(type, type =>
    {
        var memberTypes = new List<MemberInfo>();

        if (type?.IsClass ?? false)
        {
            memberTypes.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty).Cast<MemberInfo>());
            memberTypes.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField).Cast<MemberInfo>());
        }

        return memberTypes.ToArray();
    });

    private static string GetMemberName(MemberInfo member)
    {
        if (member is null)
        {
            throw new ArgumentNullException(nameof(member));
        }
        else if (((MemberTypes.Property | MemberTypes.Field) & member.MemberType) != member.MemberType)
        {
            throw new ArgumentException($"Argument must be of member type 'Property' or 'Field'", nameof(member));
        }

        var jsonProperty = member.GetCustomAttribute<JsonPropertyAttribute>();

        return string.IsNullOrEmpty(jsonProperty?.PropertyName)
            ? Template.NamingConvention.GetMemberName(member.Name)
            : Template.NamingConvention.GetMemberName(jsonProperty.PropertyName);
    }

    private sealed class ObjectProxy : DropProxy
    {
        private static IEnumerable<string> GetAllowedMembers(Type type) => GetMembers(type)
            .Select(member => GetMemberName(member));

        private readonly object data;

        public ObjectProxy(object data) : base(data, GetAllowedMembers(data?.GetType()).ToArray())
        {
            this.data = data;
        }

        public override object BeforeMethod(string method)
        {
            var member = GetMembers(data?.GetType())
                .SingleOrDefault(m => GetMemberName(m).Equals(method, StringComparison.Ordinal));

            if (member is PropertyInfo property)
                return property.GetValue(data);
            else if (member is FieldInfo field)
                return field.GetValue(data);
            else
                return base.BeforeMethod(method);
        }
    }

    private sealed class CamelCaseNamingConvention : INamingConvention
    {
        private static readonly NamingStrategy namingStrategy = new CamelCaseNamingStrategy();

        public StringComparer StringComparer
            => StringComparer.Ordinal;

        public string GetMemberName(string name)
            => namingStrategy.GetPropertyName(name, false);

        public bool OperatorEquals(string testedOperator, string referenceOperator)
            => string.Equals(GetMemberName(testedOperator), GetMemberName(referenceOperator), StringComparison.Ordinal);
    }
}
