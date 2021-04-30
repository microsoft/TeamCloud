/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Cosmos.Table;

namespace TeamCloud.Adapters.Authorization
{
    public sealed class AuthorizationEntityResolver : IAuthorizationEntityResolver
    {
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> constructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();

        private static ConstructorInfo GetConstructor(Type type) => constructorCache.GetOrAdd(type, (type) =>
        {
            var constructor = type.GetConstructors().FirstOrDefault(c => !c.GetParameters().Any());

            if (constructor is null)
            {
                // try to find a constructor that has only optional parameters and use it instead of the default one
                constructor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
            }

            return constructor;
        });

        private readonly IDataProtectionProvider dataProtectionProvider;

        public AuthorizationEntityResolver(IDataProtectionProvider dataProtectionProvider = null)
        {
            this.dataProtectionProvider = dataProtectionProvider;
        }

        public EntityResolver<AuthorizationEntity> Resolve => (string partitionKey, string rowKey, DateTimeOffset timestamp, IDictionary<string, EntityProperty> properties, string etag) =>
        {
            var entityType = Type.GetType(partitionKey, false);

            if (entityType is null)
            {
                return null;
            }
            else
            {
                var constructor = GetConstructor(entityType);
                var parameters = constructor.GetParameters().Select(p => Type.Missing).ToArray();

                var entity = (AuthorizationEntity)constructor.Invoke(parameters);

                entity.Entity.ReadEntity(properties, null, dataProtectionProvider);
                entity.Entity.PartitionKey = partitionKey;
                entity.Entity.RowKey = rowKey;
                entity.Entity.Timestamp = timestamp;
                entity.Entity.ETag = etag;

                return entity;
            }
        };
    }
}
