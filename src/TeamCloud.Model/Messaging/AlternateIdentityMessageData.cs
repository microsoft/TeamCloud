/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Messaging
{
    public sealed class AlternateIdentityMessageData
    {
        public Organization Organization { get; set; }

        public User User { get; set; }

        public string[] Services { get; set; }

        public string PortalUrl { get; set; }
    }
}
