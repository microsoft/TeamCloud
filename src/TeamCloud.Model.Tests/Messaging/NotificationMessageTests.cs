//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TeamCloud.Notification;
using TeamCloud.Notification.Smtp;

namespace TeamCloud.Model.Messaging;
public abstract class NotificationMessageTests
{
    public readonly IServiceCollection Services = new ServiceCollection();

    public NotificationMessageTests()
    {
        var options = Substitute.For<INotificationSmtpOptions>();

        options.SenderAddress.Returns("UnitTest@TeamCloud.dev");
        options.SenderName.Returns("UnitTest");
        options.Host.Returns("localhost");
        options.Port.Returns(25);

        Services.AddTeamCloudNotificationSmtpSender(options);
    }
    protected INotificationAddress CreateRecipient([CallerMemberName] string testname = nameof(NotificationMessageTests))
    {
        var recipient = Substitute.For<INotificationAddress>();

        recipient.Address.Returns($"{this.GetType().Name}.{testname}@teamcloud.dev");
        recipient.DisplayName.Returns($"UnitTest {this.GetType().Name}.{testname}");

        return recipient;
    }

    protected Task SendMessageAsync(INotificationMessage notificationMessage)
    {
        var sender = Services
            .BuildServiceProvider()
            .GetRequiredService<INotificationSmtpSender>();

        return sender.SendMessageAsync(notificationMessage);
    }
}
