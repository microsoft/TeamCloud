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
    public class DeploymentScopeDefinition : ISlug, IValidatable
    {
        public string Slug => (this as ISlug).GetSlug();

        public string DisplayName { get; set; }

        public string ManagementGroupId { get; set; }

        public bool IsDefault { get; set; }
    }
}
