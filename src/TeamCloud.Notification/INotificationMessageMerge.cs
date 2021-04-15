/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Notification
{
    public interface INotificationMessageMerge<TData> : INotificationMessage
        where TData : class, new()
    {

    }
}
