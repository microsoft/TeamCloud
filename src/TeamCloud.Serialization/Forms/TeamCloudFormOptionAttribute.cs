/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class TeamCloudFormOptionAttribute : TeamCloudFormAttribute
    {
        public static void WriteJson(IEnumerable<TeamCloudFormOptionAttribute> optionAttributes, JsonWriter writer, JsonContract contract, string property = null)
        {
            if (optionAttributes is null)
                throw new ArgumentNullException(nameof(optionAttributes));

            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (contract is null)
                throw new ArgumentNullException(nameof(contract));

            if (optionAttributes.Any())
            {
                writer.WritePropertyName("ui:options");
                writer.WriteStartObject();

                foreach (var optionAttribute in optionAttributes)
                    optionAttribute.WriteJsonValue(writer, contract, property);

                writer.WriteEndObject();
            }
        }

        private readonly string key;
        private readonly object value;

        public TeamCloudFormOptionAttribute(string key, object value) : base("options")
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));

            this.key = key;
            this.value = value;
        }

        public override void WriteJson(JsonWriter writer, JsonContract contract, string property = null)
        {
            throw new NotSupportedException($"Writing a single instance is not supported - use the static WriteJson method and pass in all attributes of type {typeof(TeamCloudFormOptionAttribute).FullName}");
        }

        protected override void WriteJsonValue(JsonWriter writer, JsonContract contract, string property = null)
        {
            writer.WritePropertyName(key);
            writer.WriteValue(value);
        }
    }
}
