/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Commands.Serialization
{
    class ExceptionConverter : JsonConverter<Exception>
    {
        private JsonSerializer InnerSerializer => JsonSerializer.CreateDefault(new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new SuppressConverterContractResolver<ExceptionConverter>() { NamingStrategy = new CamelCaseNamingStrategy() }
        });

        public override Exception ReadJson(JsonReader reader, Type objectType, Exception existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (Exception)InnerSerializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, Exception value, JsonSerializer serializer)
        {
            if (value.GetType().GetCustomAttribute<SerializableAttribute>() is null)
            {
                var serializableException = value is null
                    ? default(SerializableException)
                    : new SerializableException(value.Message, value);

                InnerSerializer.Serialize(writer, serializableException);
            }
            else
            {
                InnerSerializer.Serialize(writer, value, typeof(object));
            }
        }


        [Serializable]
        public class SerializableException : Exception
        {
            public SerializableException()
            { }

            public SerializableException(string message) : base(message)
            { }

            public SerializableException(string message, Exception inner) : base(message)
            {
                InnerExceptionType = inner?.GetType();
            }

            protected SerializableException(SerializationInfo info, StreamingContext context) : base(info, context)
            { }

            public Type InnerExceptionType { get; }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);

                info.AddValue(nameof(InnerExceptionType), InnerExceptionType);
            }
        }
    }
}
