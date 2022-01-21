/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Forms;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
public sealed class TeamCloudFormTitleAttribute : TeamCloudFormAttribute
{
    private readonly string title;

    public TeamCloudFormTitleAttribute(string title) : base("title")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException($"'{nameof(title)}' cannot be null or whitespace.", nameof(title));

        this.title = title;
    }

    protected override void WriteJsonValue(JsonWriter writer, JsonContract contract, string property = null)
    {
        writer.WriteValue(title);
    }
}
