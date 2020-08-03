/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Internal.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectDocument : ContainerDocument, IProject<UserDocument>, IEquatable<ProjectDocument>, IPopulate<Model.Data.Project>
    {
        [PartitionKey]
        public string Tenant { get; set; }

        [UniqueKey]
        public string Name { get; set; }

        public ProjectTypeDocument Type { get; set; }

        public ProjectIdentity Identity { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        [DatabaseIgnore]
        public IList<UserDocument> Users { get; set; } = new List<UserDocument>();

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();


        public bool Equals(ProjectDocument other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProjectDocument);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
