/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Data.Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProviderCommandMode
    {
        Simple,

        Extended
    }
}
