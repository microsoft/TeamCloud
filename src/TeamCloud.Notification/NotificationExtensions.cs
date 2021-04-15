/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DotLiquid;
using DotLiquid.NamingConventions;

namespace TeamCloud.Notification
{
    public static class NotificationExtensions
    {
        static NotificationExtensions()
        {
            Template.NamingConvention = new CSharpNamingConvention();
        }

        public static void Merge<TData>(this INotificationMessageMerge<TData> instance, TData data)
            where TData : class, new()
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            Hash hash = null;

            if (!string.IsNullOrEmpty(instance.Subject))
            {
                hash ??= GenerateHash();

                instance.Subject = Template
                    .Parse(instance.Subject)
                    .Render(hash);
            }

            if (!string.IsNullOrEmpty(instance.Body))
            {
                hash ??= GenerateHash();

                instance.Body = Template
                     .Parse(instance.Body)
                     .Render(hash);
            }

            Hash GenerateHash()
            {
                RegisterSafeType(data.GetType());

                var hash = new Hash();

                var properties = data.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty);

                foreach (var property in properties)
                    hash[Template.NamingConvention.GetMemberName(property.Name)] = property.GetValue(data, null);

                return hash;
            }
        }

        private static readonly HashSet<Type> RegisteredSafeTypes = new HashSet<Type>();

        private static void RegisterSafeType(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            else if (IsSupportedType(type))
            {
                var enumerableTypes = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .SelectMany(i => i.GetGenericArguments())
                    .Distinct();

                if (enumerableTypes.Any())
                {
                    foreach (var enumerableType in enumerableTypes)
                        RegisterSafeType(enumerableType);
                }
                else if (RegisteredSafeTypes.Add(type))
                {
                    Template.RegisterSafeType(type, data => new ObjectProxy(data));

                    var nestedTypes = type
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty)
                        .Select(p => p.PropertyType);

                    foreach (var nestedType in nestedTypes)
                        RegisterSafeType(nestedType);
                }
            }

            static bool IsSupportedType(Type type)
                => !type.IsValueType && type != typeof(object) && type != typeof(string);
        }

        private sealed class ObjectProxy : DropProxy
        {
            private static readonly ConcurrentDictionary<Type, string[]> AllowedMembersCache = new ConcurrentDictionary<Type, string[]>();

            private static string[] GetAllowedMembers(Type type)
            {
                if (type is null)
                    return Array.Empty<string>();

                return AllowedMembersCache.GetOrAdd(type, type =>
                {
                    var allowedMemberNames = type
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty)
                        .Select(member => Template.NamingConvention.GetMemberName(member.Name))
                        .ToArray();

                    Debug.WriteLine($"Allowed members on '{type.FullName}': {string.Join(',', allowedMemberNames)}");

                    return allowedMemberNames;
                });
            }

            public ObjectProxy(object data) : base(data, GetAllowedMembers(data?.GetType()))
            { }
        }
    }
}
