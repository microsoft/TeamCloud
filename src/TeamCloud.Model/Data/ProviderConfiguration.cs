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

        public Dictionary<string, string> Properties { get; set; }
    }
}
