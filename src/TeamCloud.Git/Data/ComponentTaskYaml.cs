/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Data
{
    public class ComponentTaskYaml
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ComponentTaskType Type =>
            Id.Equals("create", StringComparison.OrdinalIgnoreCase) ? ComponentTaskType.Create : Id.Equals("delete", StringComparison.OrdinalIgnoreCase) ? ComponentTaskType.Delete : ComponentTaskType.Custom;

        public List<YamlParameter<dynamic>> Parameters { get; set; }
    }
}
