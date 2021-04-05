/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;

namespace TeamCloud.Orchestrator.Options
{
    public interface ISmtpOptions
    {
        string Host { get; }

        int Port { get; }

        bool SSL { get; }

        string Username { get; }

        string Password { get; }

        string Sender { get; }
    }

    [Options("Notification:Smtp")]
    public sealed class TeamCloudSmtpOptions : ISmtpOptions
    {
        public string Host { get; set; }

        public int Port { get; set; } = 25;

        public bool SSL { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Sender { get; set; }
    }
}
