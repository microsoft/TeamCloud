/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class ProjectTemplateDefinition : IDisplayName, IValidatable
    {
        public string DisplayName { get; set; }

        public RepositoryDefinition Repository { get; set; }
    }
}
