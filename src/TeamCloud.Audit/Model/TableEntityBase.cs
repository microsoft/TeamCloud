/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace TeamCloud.Audit.Model
{
    public abstract class TableEntityBase : TableEntity
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<string>> ColumnOrderCache = new ConcurrentDictionary<Type, IEnumerable<string>>();
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> ReadOnlyPropertiesCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> ComplexPropertiesCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> EnumPropertiesCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        private static bool IsEdmType(Type type)
            => Enum.GetNames(typeof(EdmType)).Contains(type.Name, StringComparer.OrdinalIgnoreCase);

        private IEnumerable<string> ColumnOrder => ColumnOrderCache.GetOrAdd(GetType(), (type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .OrderBy(pi => (pi.GetCustomAttribute<ColumnAttribute>() ?? new ColumnAttribute()).Order)
            .ThenBy(pi => pi.Name)
            .Select(pi => pi.Name));

        /// <summary>
        /// Gets the complex properties.
        /// </summary>
        /// <value>The complex properties.</value>
        private IEnumerable<PropertyInfo> ReadOnlyProperties => ReadOnlyPropertiesCache.GetOrAdd(GetType(), (type) => type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(pi => !pi.IsDefined(typeof(IgnorePropertyAttribute)) && IsEdmType(pi.PropertyType) && !(pi.GetSetMethod(true)?.IsPublic ?? true)));

        /// <summary>
        /// Gets the complex properties.
        /// </summary>
        /// <value>The complex properties.</value>
        private IEnumerable<PropertyInfo> ComplexProperties => ComplexPropertiesCache.GetOrAdd(GetType(), (type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
            .Where(pi => !pi.IsDefined(typeof(IgnorePropertyAttribute)) && !pi.PropertyType.IsValueType && pi.PropertyType != typeof(string)));

        /// <summary>
        /// Gets the enum properties.
        /// </summary>
        /// <value>The complex properties.</value>
        private IEnumerable<PropertyInfo> EnumProperties => EnumPropertiesCache.GetOrAdd(GetType(), (type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
            .Where(pi => !pi.IsDefined(typeof(IgnorePropertyAttribute)) && pi.PropertyType.IsEnum));

        /// <summary>
        /// Reads the entity.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="operationContext">The operation context.</param>
        /// <exception cref="Exception">Failed to read value for property '{complexProperty.Name}</exception>
        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            if (properties is null)
                throw new ArgumentNullException(nameof(properties));

            ReadUserObject(this, properties, operationContext);

            foreach (var complexProperty in ComplexProperties)
            {
                if (properties.TryGetValue(complexProperty.Name, out EntityProperty property) && property.PropertyType == EdmType.String && !string.IsNullOrEmpty(property.StringValue))
                {
                    try
                    {
                        object value = JsonConvert.DeserializeObject(property.StringValue, complexProperty.PropertyType);

                        complexProperty.SetValue(this, value);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception($"Failed to read value for property '{complexProperty.Name}'", exc);
                    }
                }
            }

            foreach (var enumProperty in EnumProperties)
            {
                if (properties.TryGetValue(enumProperty.Name, out EntityProperty property) && property.PropertyType == EdmType.String && !string.IsNullOrEmpty(property.StringValue))
                {
                    try
                    {
                        object value = Enum.Parse(enumProperty.PropertyType, property.StringValue);

                        enumProperty.SetValue(this, value);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception($"Failed to read value for property '{enumProperty.Name}'", exc);
                    }
                }
            }

            foreach (var readOnlyProperty in ReadOnlyProperties)
            {
                if (properties.TryGetValue(readOnlyProperty.Name, out EntityProperty property))
                {
                    readOnlyProperty.SetValue(this, property.PropertyAsObject);
                }
            }
        }

        /// <summary>
        /// Writes the entity.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns></returns>
        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            if (operationContext is null)
                throw new ArgumentNullException(nameof(operationContext));

            var properties = WriteUserObject(this, operationContext);

            foreach (var complexProperty in ComplexProperties)
            {
                var value = complexProperty.GetValue(this);

                if (value != null)
                {
                    properties[complexProperty.Name] = new EntityProperty(JsonConvert.SerializeObject(value, Formatting.None));
                }
            }

            foreach (var enumProperty in EnumProperties)
            {
                properties[enumProperty.Name] = new EntityProperty(Enum.GetName(enumProperty.PropertyType, enumProperty.GetValue(this)));
            }

            foreach (var readOnlyProperty in ReadOnlyProperties)
            {
                properties[readOnlyProperty.Name] = EntityProperty.CreateEntityPropertyFromObject(readOnlyProperty.GetValue(this));
            }

            return ColumnOrder
                .Intersect(properties.Keys)
                .Concat(properties.Keys.Except(ColumnOrder))
                .ToDictionary(column => column, column => properties[column]);
        }
    }
}
