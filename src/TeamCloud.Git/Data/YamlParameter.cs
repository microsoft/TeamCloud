/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;

namespace TeamCloud.Git.Data
{
    public class YamlParameter
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public JSchemaType Type { get; set; }

        public bool Readonly { get; set; }

        public bool Required { get; set; }
    }

    public class YamlParameter<T> : YamlParameter
    {
        public T Value { get; set; }

        public T Default { get; set; }

        public string StringValue => Value?.ToString();

        public List<T> Allowed { get; set; }
    }
}
