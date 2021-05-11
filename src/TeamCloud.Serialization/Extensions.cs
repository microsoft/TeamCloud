/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

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

    }
}
