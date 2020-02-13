/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;

namespace TeamCloud.Model.Data
{
    public class ProviderConfiguration
    {
        public string TeamCloudApplicationInsightsKey { get; set; }

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
