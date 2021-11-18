/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Serialization.Resolver;

namespace TeamCloud.Serialization.Converter
{
    public abstract class TypedConverter<T> : JsonConverter<T>
    {
        private static readonly ConcurrentDictionary<Type, IContractResolver> ContractResolverCache = new ConcurrentDictionary<Type, IContractResolver>();
        private readonly IContractResolver resolver;

        protected TypedConverter(IContractResolver resolver = null)
        {
            this.resolver = resolver;
        }

        private IContractResolver GetContractResolver() => resolver ?? ContractResolverCache.GetOrAdd(this.GetType(), type
            => (IContractResolver)Activator.CreateInstance(typeof(SuppressConverterContractResolver<>).MakeGenericType(this.GetType())));

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {

            try
            {
                if (objectType is not null && !objectType.IsInterface && typeof(T).IsAssignableFrom(objectType))
                {
                    // there is no need to rely on the embedded type information if a explicit object type was requested by the serializer
                    return (T)serializer.WithContractResolver(GetContractResolver()).WithTypeNameHandling(TypeNameHandling.Auto).Deserialize(reader, objectType);
                }
                else
                {
                    // enforce deserialization as a simple object to utitlize the type information embedded in JSON and cast to the requested type
                    return (T)serializer.WithContractResolver(GetContractResolver()).WithTypeNameHandling(TypeNameHandling.Auto).Deserialize(reader, typeof(object));
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"!!! Deserializing type {typeof(T)} failed: {exc.Message}");

                throw;
            }
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
            => serializer.WithContractResolver(GetContractResolver()).WithTypeNameHandling(TypeNameHandling.Auto).Serialize(writer, value, typeof(object));
    }
}
