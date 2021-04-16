/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Messaging
{
    public sealed class WelcomeMessageData
    {
        public Organization Organization { get; set; }

        public Project Project { get; set; }

        public User User { get; set; }

        public string PortalUrl { get; set; }
    }
}
