/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class TeamCloudFormFieldAttribute : TeamCloudFormAttribute
{
    private readonly string name;

    public TeamCloudFormFieldAttribute(string name) : base("field")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));

        this.name = name;
    }

    protected override void WriteJsonValue(JsonWriter writer, JsonContract contract, string property = null)
    {
        writer.WriteValue(name);
    }
}
