/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectIdentity : ContainerDocument, IIdentifiable, IProjectContext, IEquatable<ProjectIdentity>, IValidatable
    {
        [JsonProperty(Required = Required.Always)]
        [PartitionKey]
        public string ProjectId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string DeploymentScopeId { get; set; }

        public Guid TenantId { get; set; }

        public Guid ClientId { get; set; }

        public string ClientSecret { get; set; }

        [DatabaseIgnore]
        public IEnumerable<string> RedirectUrls { get; set; }

        public Guid ObjectId { get; set; }

        public bool Equals(ProjectIdentity other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProjectIdentity);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.Ordinal);
    }
}
