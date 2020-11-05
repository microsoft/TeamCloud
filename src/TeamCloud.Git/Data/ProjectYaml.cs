/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;

namespace TeamCloud.Git.Data
{
    public class ProjectYaml
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Components { get; set; } = new List<string>();

        public List<YamlParameter<dynamic>> Parameters { get; set; }
    }
}
