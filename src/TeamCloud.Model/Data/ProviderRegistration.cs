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
        public Guid? PrincipalId { get; set; }

        public ProviderCommandMode CommandMode { get; set; } = ProviderCommandMode.Simple;

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
