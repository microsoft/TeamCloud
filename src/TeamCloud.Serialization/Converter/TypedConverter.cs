using System;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            if (contractResolver is DefaultContractResolver defaultContractResolver)
                defaultContractResolver.NamingStrategy = new TeamCloudNamingStrategy();

            return JsonSerializer.CreateDefault(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = contractResolver
            });
        });

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default(T);

            var json = JObject.Load(reader);

            var jsonTypeName = json.Children()
                .OfType<JProperty>()
                .SingleOrDefault(child => child.Name.Equals(TypePropertyName))?.Value?.ToString();

            if (string.IsNullOrEmpty(jsonTypeName))
                throw new NotSupportedException($"Could not find type information in json: {json}");

            var jsonType = Type.GetType(jsonTypeName, false);

            if (jsonType is null)
            {
                jsonType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(asm => asm.GetType(jsonTypeName, false))
                    .SingleOrDefault(type => type != null);

                if (jsonType is null)
                    throw new NotSupportedException($"Unknown type '{jsonTypeName}'");
            }

            if (!typeof(T).IsAssignableFrom(jsonType))
                throw new NotSupportedException($"Type '{typeof(T)}' is not assignable from '{jsonType}'");

            return (T)GetInnerSerializer().Deserialize(json.CreateReader(), jsonType);
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            if (value is null)
            {
                GetInnerSerializer().Serialize(writer, value);
            }
            else
            {
                var json = JObject.FromObject(value, GetInnerSerializer());

                json.AddFirst(new JProperty(TypePropertyName, string.Join(", ", value.GetType().FullName, value.GetType().Assembly.GetName().Name)));

                json.WriteTo(writer);
            }
        }
    }
}
