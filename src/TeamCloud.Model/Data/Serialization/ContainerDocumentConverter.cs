/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Serialization;

[SuppressMessage("Microsoft.Performance", "CA1812:Avoid Uninstantiated Internal Classes", Justification = "Dynamically instatiated")]
internal class ContainerDocumentConverter : JsonConverter<IContainerDocument>
{
    public override IContainerDocument ReadJson(JsonReader reader, Type objectType, IContainerDocument existingValue, bool hasExistingValue, JsonSerializer serializer)
        => (IContainerDocument)serializer.WithContractResolver<ContainerDocumentContractResolver>().Deserialize(reader, objectType);

    public override void WriteJson(JsonWriter writer, IContainerDocument value, JsonSerializer serializer)
        => serializer.WithContractResolver<ContainerDocumentContractResolver>().Serialize(writer, value);
}
