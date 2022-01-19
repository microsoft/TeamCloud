/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Data;

[JsonConverter(typeof(StringEnumConverter))]
public enum UserType
{
    User,       // AAD User
    Group,      // AAD Group
    System,     // AAD SP used by TeamCloud
    Service     // AAD ServicePrincipal
}
