using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace TeamCloud.Serialization.Converter
{
    public sealed class NameValueCollectionConverter : JsonConverter<NameValueCollection>
    {
        public override NameValueCollection ReadJson(JsonReader reader, Type objectType, [AllowNull] NameValueCollection existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue ??= new NameValueCollection();

            if (serializer is null)
                throw new ArgumentNullException(nameof(serializer));

            serializer.Deserialize<Dictionary<string, string[]>>(reader)?
                .SelectMany(item => item.Value.Select(val => new KeyValuePair<string, string>(item.Key, val)))
                .ToList().ForEach(kvp => existingValue.Add(kvp.Key, kvp.Value));

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] NameValueCollection value, JsonSerializer serializer)
        {
            if (serializer is null)
                throw new ArgumentNullException(nameof(serializer));

            var dictionary = value?.AllKeys.ToDictionary(key => key, key => value.GetValues(key));

            serializer.Serialize(writer, dictionary);
        }
    }
}
