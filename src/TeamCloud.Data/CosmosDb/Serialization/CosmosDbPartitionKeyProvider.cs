/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data.CosmosDb.Serialization
{
    public sealed class CosmosDbPartitionKeyProvider : IValueProvider
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo> PartitionKeyProperties = new ConcurrentDictionary<Type, PropertyInfo>();

        private static PropertyInfo GetPartitionKeyProperty(Type type) => PartitionKeyProperties
            .GetOrAdd(type, (type) => type.GetProperties().Where(p => p.GetCustomAttribute<PartitionKeyAttribute>() != null).SingleOrDefault());

        public object GetValue(object target)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            return GetPartitionKeyProperty(target.GetType())?.GetValue(target) ?? throw new InvalidOperationException("Properties marked with the PartitionKey attribute must have a value.");
        }

        public void SetValue(object target, object value)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            GetPartitionKeyProperty(target.GetType()).SetValue(target, value ?? throw new InvalidOperationException("Properties marked with the PartitionKey attribute must have a value."));
        }
    }
}
