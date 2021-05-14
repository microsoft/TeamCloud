/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms
{
    internal sealed class TeamCloudFormConverter<TData> : JsonConverter<TData>
        where TData : class, new()
    {
        public override bool CanRead => false;

        public override TData ReadJson(JsonReader reader, Type objectType, [AllowNull] TData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] TData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (serializer.ContractResolver.ResolveContract(typeof(TData)) is JsonObjectContract objectContract)
            {
                typeof(TData)
                    .GetCustomAttributes(false)
                    .OfType<TeamCloudFormAttribute>()
                    .ToList()
                    .ForEach(attribute => attribute.WriteJson(writer, objectContract));

                foreach (var property in objectContract.Properties)
                {
                    var attributes = property.AttributeProvider
                        .GetAttributes(false)
                        .OfType<TeamCloudFormAttribute>()
                        .ToList();

                    if (attributes.Any())
                    {
                        writer.WritePropertyName(property.PropertyName);
                        writer.WriteStartObject();
                        attributes.ForEach(attribute => attribute.WriteJson(writer, objectContract, property.PropertyName));
                        writer.WriteEndObject();
                    }
                }
            }

            writer.WriteEndObject();
        }
    }
}
