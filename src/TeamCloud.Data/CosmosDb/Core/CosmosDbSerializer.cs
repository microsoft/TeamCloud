/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.IO;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using TeamCloud.Data.CosmosDb.Serialization;
using TeamCloud.Serialization;

namespace TeamCloud.Data.CosmosDb.Core;

internal sealed class CosmosDbSerializer : CosmosSerializer
{
    private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

    private readonly JsonSerializerSettings SerializerSettings;

    public CosmosDbSerializer(IDataProtectionProvider dataProtectionProvider = null)
    {
        SerializerSettings = new TeamCloudSerializerSettings(new CosmosDbContractResolver(dataProtectionProvider));
    }

    public override T FromStream<T>(Stream stream)
    {
        if (typeof(Stream).IsAssignableFrom(typeof(T)))
            return (T)(object)stream;

        try
        {
            using var streamReader = new StreamReader(stream, DefaultEncoding, true, 1024, leaveOpen: true);
            using var jsonReader = new JsonTextReader(streamReader);

            return SerializerSettings.CreateSerializer().Deserialize<T>(jsonReader);
        }
        finally
        {
            stream?.Close();
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();

        try
        {
            using var streamWriter = new StreamWriter(stream, DefaultEncoding, 1024, leaveOpen: true);
            using var jsonWriter = new JsonTextWriter(streamWriter);

            SerializerSettings.CreateSerializer().Serialize(jsonWriter, input);

            jsonWriter.Flush();
            streamWriter.Flush();
        }
        finally
        {
            stream.Position = 0;
        }

        return stream;
    }
}
