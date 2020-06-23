/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Commands.Serialization
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid Uninstantiated Internal Classes", Justification = "Dynamically instatiated")]
    internal class CommandMessageConverter : JsonConverter<ICommandMessage>
    {
        private static readonly JsonSerializer InnerSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CommandMessageContractResolver { NamingStrategy = new TeamCloudNamingStrategy() }
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
