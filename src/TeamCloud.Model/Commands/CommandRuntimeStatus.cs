/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Commands
{
    /// <summary>
    /// Represents the possible runtime execution status values for an orchestration instance.
    /// <para>Maps directly to <see cref="Microsoft.Azure.WebJobs.Extensions.DurableTask.OrchestrationRuntimeStatus"/>.</para>
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommandRuntimeStatus
    {
        Unknown = -1,
        Running,
        Completed,
        ContinuedAsNew,
        Failed,
        Canceled,
        Terminated,
        Pending
    }
}
