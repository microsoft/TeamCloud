/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;

namespace TeamCloud.Git.Data
{
    public class ComponentTaskRunnerYaml
    {
        public string Id { get; set; }

        public Dictionary<string, string> With { get; set; } = new Dictionary<string, string>();
    }
}
