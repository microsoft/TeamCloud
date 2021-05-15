/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TeamCloudFormOrderAttribute : TeamCloudFormAttribute
    {
        private const string Wildcard = "*";
        private string[] names;

        public TeamCloudFormOrderAttribute(string name, params string[] additionalNames) : base("order")
        {
            names = additionalNames
                .Prepend(name)
                .Select(n => $"{n}".Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToArray();
        }

        protected override void WriteJsonValue(JsonWriter writer, JsonContract contract, string property = null)
        {
            writer.WriteStartArray();

            var wildcardUsed = false;

            foreach (var name in names)
            {
                if (name.Equals(Wildcard, StringComparison.OrdinalIgnoreCase))
                {
                    wildcardUsed = true;

                    writer.WriteValue(name);
                }
                else if (contract is JsonObjectContract objectContract)
                {
                    var propertyName = objectContract.Properties
                        .FirstOrDefault(p => p.PropertyName.Equals(name, StringComparison.OrdinalIgnoreCase))?
                        .PropertyName;

                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        writer.WriteValue(propertyName);
                    }
                }
            }

            if (!wildcardUsed)
            {
                writer.WriteValue(Wildcard);
            }

            writer.WriteEndArray();
        }
    }
}
