//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Messaging;

public sealed class PortalPermissionGrantMessageData
{
    public Organization Organization { get; set; }
}
