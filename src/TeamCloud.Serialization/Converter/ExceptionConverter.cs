/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;

namespace TeamCloud.Serialization.Converter
{
    public sealed class ExceptionConverter : TypedConverter<Exception>
    {
        public override void WriteJson(JsonWriter writer, Exception value, JsonSerializer serializer)
        {
            if (value is AggregateException aggregateException)
            {
                value = aggregateException.Flatten();
            }

            if (!value.IsSerializable(out var serializableException))
            {
                value = serializableException;
            }

            base.WriteJson(writer, value, serializer);
        }
    }
}
