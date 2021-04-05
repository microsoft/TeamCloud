/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Messaging
{
    public sealed class NotificationMessage
    {
        public IEnumerable<User> To { get; set; } = Enumerable.Empty<User>();

        public IEnumerable<User> Cc { get; set; } = Enumerable.Empty<User>();
    }
}
