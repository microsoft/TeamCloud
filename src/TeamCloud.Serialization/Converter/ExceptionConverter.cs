/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Serialization.Resolver;

namespace TeamCloud.Serialization.Converter
{
    public sealed class ExceptionConverter : JsonConverter<Exception>
    {
        private static readonly JsonSerializer InnerSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new SuppressConverterContractResolver<ExceptionConverter>() { NamingStrategy = new CamelCaseNamingStrategy() }
        });

        public override Exception ReadJson(JsonReader reader, Type objectType, Exception existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (Exception)InnerSerializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, Exception value, JsonSerializer serializer)
        {
            if (!value.IsSerializable(out var serializableException))
            {
                value = serializableException;
            }

            InnerSerializer.Serialize(writer, value, typeof(object));
        }
    }
}
