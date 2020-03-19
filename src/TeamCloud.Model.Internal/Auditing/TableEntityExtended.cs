/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace TeamCloud.Model.Auditing
{
    public abstract class TableEntityBase : ITableEntity
    {
        public const string ETagPropertyName = "ETag";
        public const string PartitionKeyPropertyName = "PartitionKey";
        public const string RowKeyPropertyName = "RowKey";
        public const string TimestampPropertyName = "Timestamp";

        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> ComplexPropertiesCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> EnumPropertiesCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        private static readonly MethodInfo ReflectionReadMethod = typeof(TableEntity).GetMethod("ReflectionRead", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ReflectionWriteMethod = typeof(TableEntity).GetMethod("ReflectionWrite", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Gets the table entity.
        /// </summary>
        /// <value>The table entity.</value>
        [IgnoreProperty]
        [JsonIgnore]
        public ITableEntity TableEntity => this;

        /// <summary>
        /// Gets or sets the e tag.
        /// </summary>
        /// <value>The e tag.</value>
        string ITableEntity.ETag { get; set; }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        /// <value>The partition key.</value>
        string ITableEntity.PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key.
        /// </summary>
        /// <value>The row key.</value>
        string ITableEntity.RowKey { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        DateTimeOffset ITableEntity.Timestamp { get; set; }

        /// <summary>
        /// Gets the complex properties.
        /// </summary>
        /// <value>The complex properties.</value>
        private IEnumerable<PropertyInfo> ComplexProperties => ComplexPropertiesCache.GetOrAdd(this.GetType(), (type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
            .Where(pi => !pi.IsDefined(typeof(IgnorePropertyAttribute)) && !pi.PropertyType.IsValueType && pi.PropertyType != typeof(string)));

        /// <summary>
        /// Gets the enum properties.
        /// </summary>
        /// <value>The complex properties.</value>
        private IEnumerable<PropertyInfo> EnumProperties => EnumPropertiesCache.GetOrAdd(this.GetType(), (type) => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
            .Where(pi => !pi.IsDefined(typeof(IgnorePropertyAttribute)) && pi.PropertyType.IsEnum));

        /// <summary>
        /// Reads the entity.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="operationContext">The operation context.</param>
        /// <exception cref="Exception">Failed to read value for property '{complexProperty.Name}</exception>
        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            if (properties is null)
                throw new ArgumentNullException(nameof(properties));

            ReflectionReadMethod.Invoke(null, new object[] { this, properties, operationContext });

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
        }

        /// <summary>
        /// Writes the entity.
        /// </summary>
        /// <param name="operationContext">The operation context.</param>
        /// <returns></returns>
        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            if (operationContext is null)
                throw new ArgumentNullException(nameof(operationContext));

            var properties = (IDictionary<string, EntityProperty>)ReflectionWriteMethod.Invoke(null, new object[] { this, operationContext });

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

            return properties;
        }
    }
}
