/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using YamlDotNet.Serialization;

namespace TeamCloud.API.Formatters
{
    public class YamlInputFormatter : TextInputFormatter
    {
        private readonly IDeserializer deserializer;

        public YamlInputFormatter(IDeserializer deserializer)
        {
            this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationYaml);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextYaml);
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (encoding is null) throw new ArgumentNullException(nameof(encoding));

            var request = context.HttpContext.Request;

            using var streamReader = context.ReaderFactory(request.Body, encoding);

            var type = context.ModelType;

            try
            {
                var model = deserializer.Deserialize(streamReader, type);

                return InputFormatterResult.SuccessAsync(model);
            }
            catch
            {
                return InputFormatterResult.FailureAsync();
            }
        }
    }
}

internal class MediaTypeHeaderValues
{
    public static readonly MediaTypeHeaderValue ApplicationYaml
        = MediaTypeHeaderValue.Parse("application/x-yaml").CopyAsReadOnly();

    public static readonly MediaTypeHeaderValue TextYaml
        = MediaTypeHeaderValue.Parse("text/yaml").CopyAsReadOnly();
}

