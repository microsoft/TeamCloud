//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using TeamCloud.Model.Data;
using TeamCloud.Notification.Smtp;
using Xunit;

namespace TeamCloud.Model.Messaging;
public class PortalPermissionGrantMessageTests : NotificationMessageTests
{
    [Fact]
    public async Task CreateMessage()
    {
        var messageData = new PortalPermissionGrantMessageData()
        {
            Organization = new Organization()
            {
                DisplayName = "Test-Organization",
                Portal = PortalType.Backstage
            }
        };

        var message = NotificationMessage.Create<PortalPermissionGrantMessage>(CreateRecipient());

        message.Merge(messageData);

        await SendMessageAsync(message).ConfigureAwait(false);
    }
}
