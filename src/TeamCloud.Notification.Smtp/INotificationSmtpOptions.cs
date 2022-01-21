/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Notification.Smtp;

public interface INotificationSmtpOptions
{
    string Host { get; }

    int Port { get; }

    bool SSL { get; }

    string Username { get; }

    string Password { get; }

    string SenderAddress { get; }

    string SenderName { get; }
}
