/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms
{
    public abstract class TeamCloudFormAttribute : Attribute
    {
        protected TeamCloudFormAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));

            Name = $"ui:{name}".Replace(" ", "", StringComparison.OrdinalIgnoreCase);
        }

        public string Name { get; }

        public void WriteJson(JsonWriter writer, JsonContract contract, string property = null)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (contract is null)
                throw new ArgumentNullException(nameof(contract));

            writer.WritePropertyName(this.Name);

            this.WriteJsonValue(writer, contract, property);
        }

        protected abstract void WriteJsonValue(JsonWriter writer, JsonContract contract, string property = null);
    }
}
