/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using YamlDotNet.Serialization;

namespace TeamCloud.API
{
    public class YamlOutputFormatter : TextOutputFormatter
    {
        private readonly ISerializer serializer;

        public YamlOutputFormatter(ISerializer serializer)
        {
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationYaml);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextYaml);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (selectedEncoding == null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            var response = context.HttpContext.Response;

            using var writer = context.WriterFactory(response.Body, selectedEncoding);

            WriteObject(writer, context.Object);

            await writer.FlushAsync();
        }

        private void WriteObject(TextWriter writer, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            serializer.Serialize(writer, value);
        }
    }
}