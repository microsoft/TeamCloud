/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Commands.Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommandAction
    {
        Unknown,
        Create,
        Update,
        Delete,
        Register,
        Custom
    }
}
