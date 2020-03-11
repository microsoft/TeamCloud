using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Commands.Serialization
{
    class CommandMessageConverter : JsonConverter<ICommandMessage>
    {
        private JsonSerializer InnerSerializer => JsonSerializer.CreateDefault(new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CommandMessageContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        });

        public override ICommandMessage ReadJson(JsonReader reader, Type objectType, ICommandMessage existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (ICommandMessage)InnerSerializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, ICommandMessage value, JsonSerializer serializer)
        {
            // serialize as type object to ensure type information on root level
            InnerSerializer.Serialize(writer, value, typeof(object));
        }
    }
}
