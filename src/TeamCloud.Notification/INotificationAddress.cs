/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Notification;

public interface INotificationAddress
{
    string Address { get; }

    string DisplayName { get; }
}
