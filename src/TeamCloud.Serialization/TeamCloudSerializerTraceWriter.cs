/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization
{
    public sealed class TeamCloudSerializerTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter => TraceLevel.Verbose;

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            Debug.WriteLine($"JsonNET: {level} - {message} ({ex})");
        }
    }
}
