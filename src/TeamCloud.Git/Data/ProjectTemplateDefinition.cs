/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Data
{
    public class ProjectTemplateDefinition
    {
        public ProjectTemplate Template { get; set; }

        public List<ComponentOffer> Offers { get; set; }
    }
}
