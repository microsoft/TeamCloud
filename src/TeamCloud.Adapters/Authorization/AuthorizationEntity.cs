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
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.Adapters.Authorization
{
    public abstract class AuthorizationEntity : ITableEntity
    {
        public static string GetEntityId(DeploymentScope deploymentScope)
        {
            var entityId = Guid.Empty;

            if (deploymentScope is not null)
            {
                var result = Merge(Guid.Parse(deploymentScope.Organization), Guid.Parse(deploymentScope.Id));

                entityId = new Guid(result.ToArray());
            }

            return entityId.ToString();

            static IEnumerable<byte> Merge(Guid guid1, Guid guid2)
            {
                var buffer1 = guid1.ToByteArray();
                var buffer2 = guid2.ToByteArray();

                for (int i = 0; i < buffer1.Length; i++)
                    yield return (byte)(buffer1[i] ^ buffer2[i]);
            }
        }

        internal AuthorizationEntity()
        { }

        private static bool IsEdmType(Type type)
            => Enum.GetNames(typeof(EdmType)).Contains(type.Name, StringComparer.OrdinalIgnoreCase) || type.IsEnum;

        private static bool IsComplexType(Type type)
            => type.IsClass && type != typeof(string);

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
                .Where(pi => !pi.IsDefined(typeof(IgnorePropertyAttribute)) && (IsEdmType(pi.PropertyType) || IsComplexType(pi.PropertyType)) && (pi.CanWrite || pi.GetSetMethod(true) is not null))));
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
                            throw new Exception($"Failed to parse enum value for property '{entityProperty.Name}'", exc);
                        }
                    }
                    else if (IsComplexType(entityProperty.PropertyType) && property.PropertyType == EdmType.String && !string.IsNullOrWhiteSpace(property.StringValue))
                    {
                        try
                        {
                            value = string.IsNullOrWhiteSpace(property.StringValue)
                                ? default
                                : TeamCloudSerialize.DeserializeObject(property.StringValue, entityProperty.PropertyType);
                        }
                        catch (Exception exc)
                        {
                            throw new Exception($"Failed to deserialize complex value for property '{entityProperty.Name}'", exc);
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
                else if (IsComplexType(entityProperty.PropertyType))
                {
                    var value = entityProperty.GetValue(this);

                    var json = value is null
                        ? default
                        : TeamCloudSerialize.SerializeObject(value as object);

                    properties[entityProperty.Name] = EntityProperty.GeneratePropertyForString(json);
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
