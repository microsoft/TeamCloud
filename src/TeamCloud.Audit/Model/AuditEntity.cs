/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace TeamCloud.Audit.Model
{
    public abstract class AuditEntity : ITableEntity
    {
        public const string PartitionKeyName = nameof(TableEntity.PartitionKey);
        public const string RowKeyName = nameof(TableEntity.RowKey);
        public const string TimestampName = nameof(TableEntity.Timestamp);
        public const string ETag = nameof(TableEntity.ETag);

        private static bool IsEdmType(Type type)
            => Enum.GetNames(typeof(EdmType)).Contains(type.Name, StringComparer.OrdinalIgnoreCase) || type.IsEnum;

        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> ResolvePropertiesCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        private static IEnumerable<PropertyInfo> ResolveProperties(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var properties = type.BaseType == typeof(object)
                ? Enumerable.Empty<PropertyInfo>()
                : ResolveProperties(type.BaseType);

            return properties.Union(ResolvePropertiesCache.GetOrAdd(type, (type) => type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => !pi.IsDefined(typeof(IgnorePropertyAttribute)) && IsEdmType(pi.PropertyType) && (pi.CanWrite || pi.GetSetMethod(true) != null))));
        }

        private IEnumerable<PropertyInfo> EntityProperties => ResolveProperties(this.GetType());

        [JsonIgnore]
        public ITableEntity Entity => this;

        string ITableEntity.PartitionKey { get; set; }

        string ITableEntity.RowKey { get; set; }

        DateTimeOffset ITableEntity.Timestamp { get; set; }

        string ITableEntity.ETag { get; set; }

        void ITableEntity.ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            if (properties is null)
                throw new ArgumentNullException(nameof(properties));

            TableEntity.ReadUserObject(this, properties, operationContext);

            foreach (var entityProperty in EntityProperties)
            {
                if (properties.TryGetValue(entityProperty.Name, out EntityProperty property))
                {
                    var value = property.PropertyAsObject;

                    if (entityProperty.PropertyType.IsEnum && property.PropertyType == EdmType.String && !string.IsNullOrWhiteSpace(property.StringValue))
                    {
                        try
                        {
                            value = Enum.Parse(entityProperty.PropertyType, property.StringValue);
                        }
                        catch (Exception exc)
                        {
                            throw new Exception($"Failed to read value for property '{entityProperty.Name}'", exc);
                        }
                    }

                    entityProperty.SetValue(this, value);
                }
            }
        }

        IDictionary<string, EntityProperty> ITableEntity.WriteEntity(OperationContext operationContext)
        {
            if (operationContext is null)
                throw new ArgumentNullException(nameof(operationContext));

            var properties = TableEntity.WriteUserObject(this, operationContext);

            foreach (var entityProperty in EntityProperties)
            {
                if (entityProperty.PropertyType.IsEnum)
                {
                    properties[entityProperty.Name] = new EntityProperty(Enum.GetName(entityProperty.PropertyType, entityProperty.GetValue(this)));
                }
                else
                {
                    properties[entityProperty.Name] = EntityProperty.CreateEntityPropertyFromObject(entityProperty.GetValue(this));
                }
            }

            return properties;
        }
    }
}
