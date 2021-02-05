/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Serialization
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid Uninstantiated Internal Classes", Justification = "Dynamically instatiated")]
    internal class ReferenceLinksContainerConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => typeof(ReferenceLinksContainer<,>).IsAssignableFrom(objectType.GetGenericTypeDefinition());

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => serializer.WithContractResolver<ReferenceLinksContainerContractResolver>().Deserialize(reader, objectType);


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.WithContractResolver<ReferenceLinksContainerContractResolver>().Serialize(writer, value);
    }
}
