/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;

namespace TeamCloud.Notification
{
    public interface INotificationMessage
    {
        IEnumerable<INotificationRecipient> Recipients { get; set; }

        string Subject { get; set; }

        string Body { get; set; }

        bool Html { get; set; }
    }

    public interface INotificationMessage<TData> : INotificationMessage
        where TData : class
    {
        void Merge(TData data);
    }
}
