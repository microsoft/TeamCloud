/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Model.Commands.Core
{
    public static class CommandRuntimeStatusExtensions
    {
        public static bool IsActive(this CommandRuntimeStatus status)
            => status == CommandRuntimeStatus.ContinuedAsNew
            || status == CommandRuntimeStatus.Pending
            || status == CommandRuntimeStatus.Running;

        public static bool IsFinal(this CommandRuntimeStatus status)
            => status == CommandRuntimeStatus.Canceled
            || status == CommandRuntimeStatus.Completed
            || status == CommandRuntimeStatus.Failed
            || status == CommandRuntimeStatus.Terminated;

        public static bool IsUnknown(this CommandRuntimeStatus status)
            => status == CommandRuntimeStatus.Unknown;

        public static bool IsFinal(this CommandRuntimeStatus? status)
            => status.GetValueOrDefault(CommandRuntimeStatus.Unknown).IsFinal();

        public static bool IsActive(this CommandRuntimeStatus? status)
            => status.GetValueOrDefault(CommandRuntimeStatus.Unknown).IsActive();

        public static bool IsUnknown(this CommandRuntimeStatus? status)
            => status.GetValueOrDefault(CommandRuntimeStatus.Unknown).IsUnknown();
    }
}
