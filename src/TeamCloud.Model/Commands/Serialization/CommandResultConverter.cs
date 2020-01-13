/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Commands.Serialization
{
    class CommandResultConverter : JsonConverter<ICommandResult>
    {
        private JsonSerializer InnerSerializer => JsonSerializer.CreateDefault(new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CommandResultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        });

        public override ICommandResult ReadJson(JsonReader reader, Type objectType, ICommandResult existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (ICommandResult)InnerSerializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, ICommandResult value, JsonSerializer serializer)
        {
            // serialize as type object to ensure type information on root level
            InnerSerializer.Serialize(writer, value, typeof(object));
        }
    }
}
