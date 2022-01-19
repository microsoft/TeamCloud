/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;

namespace TeamCloud.Data.CosmosDb.Serialization;

internal sealed class CosmosDbTimestampConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(DateTime);

    public override bool CanRead => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return DateTimeOffset.FromUnixTimeSeconds((long)reader.Value).UtcDateTime;
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
