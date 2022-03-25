//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCloud.Notification;
using TeamCloud.Templates;

namespace TeamCloud.Model.Messaging;
public sealed class PortalPermissionGrantMessage : NotificationMessage, INotificationMessage<PortalPermissionGrantMessageData>
{
    public void Merge(PortalPermissionGrantMessageData data)
    {
        Body = Body?.Merge(data);
    }
}