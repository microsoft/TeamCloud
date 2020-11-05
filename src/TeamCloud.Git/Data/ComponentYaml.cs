/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Data
{
    public class ComponentYaml
    {
        public string Description { get; set; }

        public ComponentType Type { get; set; }

        public ComponentScope Scope { get; set; } = ComponentScope.Project;

        public List<YamlParameter<dynamic>> Parameters { get; set; }

        public string Provider { get; set; }
    }
}
