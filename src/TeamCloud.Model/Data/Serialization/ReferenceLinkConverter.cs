using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Serialization
{
    internal class ReferenceLinkConverter : JsonConverter<ReferenceLink>
    {
        private static readonly JsonSerializer InnerSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new ReferenceLinkContractResolver { NamingStrategy = new TeamCloudNamingStrategy() }
        });

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
}
