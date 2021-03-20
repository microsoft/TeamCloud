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
        public string Name { get; set; }

        public string Description { get; set; }

        public ComponentType Type { get; set; }

        public List<ComponentTaskYaml> Tasks { get; set; }

        public ComponentTaskRunnerYaml TaskRunner { get; set; }

        public List<ComponentPermissionYaml> Permissions { get; set; }

        public List<YamlParameter<dynamic>> Parameters { get; set; }

    }
}
