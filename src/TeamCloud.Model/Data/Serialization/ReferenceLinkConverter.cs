/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Serialization;

[SuppressMessage("Microsoft.Performance", "CA1812:Avoid Uninstantiated Internal Classes", Justification = "Dynamically instatiated")]
internal class ReferenceLinkConverter : JsonConverter<ReferenceLink>
{
    private static readonly JsonSerializer InnerSerializer = TeamCloudSerializerSettings
        .Create<ReferenceLinkContractResolver>()
        .CreateSerializer();

    public override ReferenceLink ReadJson(JsonReader reader, Type objectType, [AllowNull] ReferenceLink existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return InnerSerializer.Deserialize<ReferenceLink>(reader);
    }

    public override void WriteJson(JsonWriter writer, [AllowNull] ReferenceLink value, JsonSerializer serializer)
    {
        if (string.IsNullOrEmpty(value?.HRef))
        {
            writer.WriteNull();
        }
        else
        {
            InnerSerializer.Serialize(writer, value);
        }
    }
}
