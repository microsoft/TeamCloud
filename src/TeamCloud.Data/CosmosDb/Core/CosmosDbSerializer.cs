using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamCloud.Data.CosmosDb.Serialization;

namespace TeamCloud.Data.CosmosDb.Core
{
    internal sealed class CosmosDbSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        private readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ContractResolver = new CosmosDbContractResolver()
        };

        private JsonSerializer GetSerializer()
             => JsonSerializer.Create(SerializerSettings);

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                    return (T)(object)stream;

                TraceStream(stream);

                using var streamReader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(streamReader);

                return GetSerializer().Deserialize<T>(jsonReader);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var stream = new MemoryStream();

            try
            {
                using var streamWriter = new StreamWriter(stream, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true);
                using var jsonWriter = new JsonTextWriter(streamWriter);

                GetSerializer().Serialize(jsonWriter, input);

                jsonWriter.Flush();
                streamWriter.Flush();
            }
            finally
            {
                stream.Position = 0;
            }

            TraceStream(stream);

            return stream;
        }

        [Conditional("DEBUG")]
        private void TraceStream(Stream stream, [CallerMemberName] string memberName = default)
        {
            try
            {
                using var streamReader = new StreamReader(stream, DefaultEncoding, true, 1024, leaveOpen: true);
                using var jsonReader = new JsonTextReader(streamReader);

                Debug.WriteLine($"{memberName ?? "UNKNOWN"}: {JObject.ReadFrom(jsonReader).ToString(Formatting.None)}");
            }
            finally
            {
                stream.Position = 0;
            }
        }
    }
}
