using Newtonsoft.Json;
using System;
using TeamCloud.API.Data.Results;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data.Serialization
{
    public class DataResultConverter : JsonConverter
    {
        private readonly JsonSerializer internalSerializer = TeamCloudSerializerSettings
            .Create<DataResultContractResolver>()
            .CreateSerializer();

        public override bool CanConvert(Type objectType)
            => typeof(DataResult<>).IsAssignableFrom(objectType?.GetGenericTypeDefinition() ?? throw new ArgumentNullException(nameof(objectType)));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => serializer.WithContractResolver<DataResultContractResolver>().Deserialize(reader, objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.WithContractResolver<DataResultContractResolver>().Serialize(writer, value);
    }
}
