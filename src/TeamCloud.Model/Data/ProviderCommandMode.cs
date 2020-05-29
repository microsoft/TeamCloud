/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProviderCommandMode
    {
        Simple,

        Extended
    }
}
