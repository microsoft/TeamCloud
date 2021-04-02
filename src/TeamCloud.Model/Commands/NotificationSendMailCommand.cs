using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Messaging;

namespace TeamCloud.Model.Commands
{
    public sealed class NotificationSendMailCommand : CustomCommand<NotificationMessage, NotificationSendMailCommandResult>
    {
        public NotificationSendMailCommand(User user, NotificationMessage payload) : base(user, payload)
        { }
    }
}
