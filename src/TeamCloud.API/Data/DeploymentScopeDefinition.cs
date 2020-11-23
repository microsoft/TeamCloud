/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class DeploymentScopeDefinition : ISlug, IValidatable
    {
        public string Slug => (this as ISlug).GetSlug();

        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        public bool IsDefault { get; set; }

        public string ManagementGroupId { get; set; }

        public List<Guid> SubscriptionIds { get; set; }
    }
}
