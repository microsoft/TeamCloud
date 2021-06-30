/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Notification;
using TeamCloud.Templates;

namespace TeamCloud.Model.Messaging
{
    public sealed class AlternateIdentityMessage : NotificationMessage, INotificationMessage<AlternateIdentityMessageData>
    {
        public void Merge(AlternateIdentityMessageData data)
        {
            Body = Body?.Merge(data);
        }
    }
}
