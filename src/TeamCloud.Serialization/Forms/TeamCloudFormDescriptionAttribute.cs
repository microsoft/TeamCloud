/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public sealed class TeamCloudFormDescriptionAttribute : TeamCloudFormAttribute
    {
        private readonly string description;

        public TeamCloudFormDescriptionAttribute(string description) : base("description")
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException($"'{nameof(description)}' cannot be null or whitespace.", nameof(description));

            this.description = description;
        }

        protected override void WriteJsonValue(JsonWriter writer, JsonContract contract, string property = null)
        {
            writer.WriteValue(description);
        }
    }
}
