/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Data.CosmosDb.Serialization
{
    internal sealed class CosmosDbContractResolver : TeamCloudContractResolver
    {
        private static readonly IReadOnlyDictionary<string, (string, JsonConverter)> MetadataPropertyMap = new Dictionary<string, (string, JsonConverter)>()
        {
            { nameof(IContainerDocument.ETag), ("_etag", null) },
            { nameof(IContainerDocument.Timestamp), ("_ts", new CosmosDbTimestampConverter()) }
        };

        public CosmosDbContractResolver(IDataProtectionProvider dataProtectionProvider = null)
            : base(dataProtectionProvider)
        { }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (typeof(IContainerDocument).IsAssignableFrom(objectType))
            {
                contract.Converter = null;
            }

            return contract;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            if (typeof(IContainerDocument).IsAssignableFrom(type))
            {
                // metadata properties must not be serialized at all
                // furthermore we need to change to property name according
                // to the name used by cosmos db

                foreach (var metaProperty in base.CreateProperties(typeof(IContainerDocument), memberSerialization))
                {
                    if (MetadataPropertyMap.TryGetValue(metaProperty.UnderlyingName, out (string, JsonConverter) value))
                    {
                        metaProperty.PropertyName = value.Item1;
                        metaProperty.Converter = value.Item2;
                    }

                    if (properties.Any(p => p.PropertyName == metaProperty.PropertyName))
                        continue;

                    properties.Add(metaProperty);
                }
            }

            return properties;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (member.GetCustomAttribute<PartitionKeyAttribute>() != null)
            {
                // properties marked with the PartitionKeyAttribute
                // must use a special converter to ensure the partionkey
                // matches the given partionkey (see ctor) on read and write

                property.ValueProvider = new CosmosDbPartitionKeyProvider();
            }
            else if (member.GetCustomAttribute<DatabaseIgnoreAttribute>() != null)
            {
                // properties marked with the DatabaseIgnoreAttribute
                // must not be serialized and persisted to the database

                property.ShouldSerialize = (value) => false;
            }

            return property;
        }
    }
}
