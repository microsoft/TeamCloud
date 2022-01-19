/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.API.Data.Results;

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultErrorCode
{
    Unknown,
    Failed,
    Conflict,
    NotFound,
    ServerError,
    ValidationError,
    Unauthorized,
    Forbidden
}
