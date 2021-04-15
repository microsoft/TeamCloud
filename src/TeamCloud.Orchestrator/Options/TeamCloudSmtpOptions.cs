/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Notification.Smtp;

namespace TeamCloud.Orchestrator.Options
{
    [Options("Notification:Smtp")]
    public sealed class TeamCloudSmtpOptions : INotificationSmtpOptions
    {
        public string Host { get; set; }

        public int Port { get; set; } = 25;

        public bool SSL { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SenderAddress { get; set; }

        public string SenderName { get; set; }
    }
}
