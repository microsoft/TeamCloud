/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class ProjectTemplateDefinition
    {
        public string DisplayName { get; set; }

        public RepositoryDefinition Repository { get; set; }
    }
}
