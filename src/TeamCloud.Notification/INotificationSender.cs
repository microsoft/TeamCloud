/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;

namespace TeamCloud.Notification
{
    public interface INotificationSender
    {
        Task SendMessageAsync(INotificationMessage notificationMessage);
    }
}
