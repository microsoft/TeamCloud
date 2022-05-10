/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms;

[AttributeUsage(AttributeTargets.Property)]
public sealed class TeamCloudFormHiddenAttribute : TeamCloudFormAttribute
{
    public TeamCloudFormHiddenAttribute() : base(nameof(TeamCloudFormHiddenAttribute))
    { }

    protected override void WriteJsonValue(JsonWriter writer, JsonContract contract, string property = null)
    { }
}