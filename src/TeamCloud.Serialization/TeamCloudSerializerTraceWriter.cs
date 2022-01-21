/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;

namespace TeamCloud.Serialization;

public sealed class TeamCloudSerializerTraceWriter : ITraceWriter
{
    public TraceLevel LevelFilter => TraceLevel.Warning;

    public void Trace(TraceLevel level, string message, Exception ex)
    {
        Debug.WriteLine($"JsonNET: {level} - {message} {ex}".Trim());
    }
}
