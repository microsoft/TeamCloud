/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class OrganizationDefinition
    {
        // public string Id => Name.ToLowerInvariant().Replace(' ', '_').Replace('-', '_');

        public string Slug => Name
            .ToLowerInvariant()
            .Replace(' ', '_')
            .Replace('-', '_');

        public string Name { get; set; }

        public string Tenant { get; set; }
    }
}
