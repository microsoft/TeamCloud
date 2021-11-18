/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization
{
    public static class Extensions
    {
        public static bool IsSerializable(this Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));

            if (typeof(SerializableException).IsAssignableFrom(exception.GetType()))
                return true;

            var jsonSerializable = !(exception.GetType().GetCustomAttribute<SerializableAttribute>() is null);

            return jsonSerializable && (exception.InnerException?.IsSerializable() ?? true);
        }

        public static bool IsSerializable(this Exception exception, out Exception serializableException)
        {
            var isSerializable = exception.IsSerializable();

            serializableException = (isSerializable ? null : new SerializableException(exception));

            return isSerializable;
        }

        public static bool TrySelectToken(this JToken instance, string path, out JToken token)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            token = instance.SelectToken(path);

            return token is not null;
        }

        public static Exception AsSerializable(this Exception exception)
            => exception.IsSerializable() ? exception : new SerializableException(exception);

        public static JsonSerializer CreateSerializer(this JsonSerializerSettings jsonSerializerSettings)
            => JsonSerializer.CreateDefault(jsonSerializerSettings ?? throw new ArgumentNullException(nameof(jsonSerializerSettings)));

        public static JsonSerializer WithContractResolver<TContractResolver>(this JsonSerializer jsonSerializer)
            where TContractResolver : IContractResolver, new()
            => jsonSerializer.WithContractResolver(Activator.CreateInstance<TContractResolver>());

        public static JsonSerializer WithContractResolver(this JsonSerializer jsonSerializer, IContractResolver contractResolver)
        {
            if (jsonSerializer is null)
            {
                throw new ArgumentNullException(nameof(jsonSerializer));
            }

            if (contractResolver is null)
            {
                throw new ArgumentNullException(nameof(contractResolver));
            }

            jsonSerializer.ContractResolver = contractResolver;

            return jsonSerializer;
        }

        public static JsonSerializer WithTypeNameHandling(this JsonSerializer jsonSerializer, TypeNameHandling typeNameHandling)
        {
            if (jsonSerializer is null)
            {
                throw new ArgumentNullException(nameof(jsonSerializer));
            }

            jsonSerializer.TypeNameHandling = typeNameHandling;

            return jsonSerializer;
        }

        public static string ToString(this JToken token, Formatting formatting)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var sb = new StringBuilder();

            using var sw = new StringWriter(sb);
            using var jw = new JsonTextWriter(sw) { Formatting = formatting };

            token.WriteTo(jw);

            return sb.ToString();
        }
    }
}
