/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TeamCloud.Model.Data.Core;

[AttributeUsage(AttributeTargets.Class)]
internal class ContainerPathAttribute : Attribute
{
    private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyCache = new();
    private static readonly ConcurrentDictionary<string, FieldInfo> FieldCache = new();

    private static readonly Regex TokenExpression = new("[^{}]+(?=})");
    private static readonly Regex SanitizeGuidExpression = new(@"\{[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}\}");

    public ContainerPathAttribute(string pathTemplate)
    {
        if (string.IsNullOrWhiteSpace(pathTemplate))
            throw new ArgumentException($"'{nameof(pathTemplate)}' cannot be null or whitespace.", nameof(pathTemplate));

        PathTemplate = pathTemplate;
    }

    public string PathTemplate { get; }

    public string ResolvePath(IContainerDocument containerDocument)
    {
        if (containerDocument is null)
            throw new ArgumentNullException(nameof(containerDocument));

        var path = TokenExpression.Replace(PathTemplate, match =>
        {
            var key = $"{match.Value}@{containerDocument.GetType()}";

            var property = PropertyCache.GetOrAdd(key, _ =>
            {
                var propertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;

                var property = containerDocument.GetType()
                    .GetProperty(match.Value, propertyFlags);

                return property ?? containerDocument.GetType()
                    .GetProperties(propertyFlags)
                    .FirstOrDefault(p => p.Name.Equals(match.Value, StringComparison.OrdinalIgnoreCase));
            });

            if (property is not null)
            {
                return $"{property.GetValue(containerDocument)}";
            }

            var field = FieldCache.GetOrAdd(key, _ =>
            {
                var fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField;

                var field = containerDocument.GetType()
                    .GetField(match.Value, fieldFlags);

                return field ?? containerDocument.GetType()
                    .GetFields(fieldFlags)
                    .FirstOrDefault(f => f.Name.Equals(match.Value, StringComparison.OrdinalIgnoreCase));
            });

            if (field is not null)
            {
                return $"{field.GetValue(containerDocument)}";
            }

            return default;
        });

        return SanitizeGuidExpression.Replace(path, match => Guid.Parse(match.Value).ToString());
    }
}
