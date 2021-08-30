/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Common
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TaskState
    {
        Pending,

        Initializing,

        Processing,

        Succeeded,

        Failed
    }

    public static class TaskStateExtensions
    {
        public static bool IsFinal(this TaskState taskState)
            => taskState == TaskState.Succeeded || taskState == TaskState.Failed;

        public static bool IsActive(this TaskState taskState)
            => !taskState.IsFinal();
    }
}
