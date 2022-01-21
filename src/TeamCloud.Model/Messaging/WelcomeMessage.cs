/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Notification;
using TeamCloud.Templates;

namespace TeamCloud.Model.Messaging;

public sealed class WelcomeMessage : NotificationMessage, INotificationMessage<WelcomeMessageData>
{
    public void Merge(WelcomeMessageData data = null)
    {
        Body = Body?.Merge(data);
    }
}
