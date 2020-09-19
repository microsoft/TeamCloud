using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Serialization.Resolver;

namespace TeamCloud.Serialization.Converter
{
    public abstract class TypedConverter<T> : JsonConverter<T>
    {
        private const string TypePropertyName = "$type";

        private static readonly ConcurrentDictionary<Type, JsonSerializer> InnerSerializerCache = new ConcurrentDictionary<Type, JsonSerializer>();

        private JsonSerializer GetInnerSerializer() => InnerSerializerCache.GetOrAdd(this.GetType(), type =>
        {
            var contractResolver = (IContractResolver)Activator
                .CreateInstance(typeof(SuppressContractResolver<>).MakeGenericType(this.GetType()));

            return JsonSerializer.CreateDefault(TeamCloudSerializerSettings.Create(contractResolver));
        });

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
            => (T)GetInnerSerializer().Deserialize(reader, objectType);

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
            => GetInnerSerializer().Serialize(writer, value, typeof(object));
    }
}
