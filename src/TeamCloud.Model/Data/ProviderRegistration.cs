/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;

namespace TeamCloud.Model.Data
{
    public class ProviderRegistration
    {
        public Guid PricipalId { get; set; }

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}
