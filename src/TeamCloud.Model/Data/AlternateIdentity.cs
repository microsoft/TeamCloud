﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public sealed class AlternateIdentity
{
    public string Login { get; set; }
}
