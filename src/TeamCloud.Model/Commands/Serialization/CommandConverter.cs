/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Commands.Serialization
{
    class CommandConverter : JsonConverter<ICommand>
    {
        private JsonSerializer InnerSerializer => JsonSerializer.CreateDefault(new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CommandContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
        });

        public override ICommand ReadJson(JsonReader reader, Type objectType, ICommand existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (ICommand)InnerSerializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, ICommand value, JsonSerializer serializer)
        {
            // serialize as type object to ensure type information on root level
            InnerSerializer.Serialize(writer, value, typeof(object));
        }
    }
}
