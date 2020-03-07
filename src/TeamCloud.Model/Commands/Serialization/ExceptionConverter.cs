/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Commands.Serialization
{
    class ExceptionConverter : JsonConverter<Exception>
    {
        private JsonSerializer InnerSerializer => JsonSerializer.CreateDefault(new JsonSerializerSettings()
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
            if (!value.IsJsonSerializable())
            {
                var serializableException = value is null
                    ? default(CommandException)
                    : new CommandException(value.Message);

                value = serializableException;
            }

            InnerSerializer.Serialize(writer, value, typeof(object));
        }
    }
}
