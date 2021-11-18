using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TeamCloudFormFormatAttribute : TeamCloudFormAttribute
    {
        private readonly string format;

        public TeamCloudFormFormatAttribute(string format) : base("format")
        {
            if (string.IsNullOrWhiteSpace(format))
                throw new ArgumentException($"'{nameof(format)}' cannot be null or whitespace.", nameof(format));

            this.format = format;
        }

        protected override void WriteJsonValue(JsonWriter writer, JsonContract contract, string property = null)
        {
            writer.WriteValue(format);
        }
    }
}
