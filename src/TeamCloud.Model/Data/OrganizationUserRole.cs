﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Data;

[JsonConverter(typeof(StringEnumConverter))]
public enum OrganizationUserRole
{
    None,
    Member,
    Admin,
    Owner,
    Adapter
}
