/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Data.Serialization
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid Uninstantiated Internal Classes", Justification = "Dynamically instatiated")]
    internal class ContainerDocumentConverter : JsonConverter<IContainerDocument>
    {
        private static readonly JsonSerializer InnerSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new ContainerDocumentContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
        });

        public override IContainerDocument ReadJson(JsonReader reader, Type objectType, IContainerDocument existingValue, bool hasExistingValue, JsonSerializer serializer)
            => (IContainerDocument)InnerSerializer.Deserialize(reader, objectType);

        public override void WriteJson(JsonWriter writer, IContainerDocument value, JsonSerializer serializer)
            => InnerSerializer.Serialize(writer, value);
    }
}
