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
    internal class CommandResultConverter : JsonConverter<ICommandResult>
    {
        private static readonly JsonSerializer InnerSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CommandResultContractResolver { NamingStrategy = new TeamCloudNamingStrategy() }
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
