/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;

namespace TeamCloud.Model.Data
{
    public class ComponentTaskRunner
    {
        public string Id { get; set; }

        public Dictionary<string, string> With { get; set; } = new Dictionary<string, string>();
    }
}
