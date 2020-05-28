/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class Project : ContainerDocument, IEquatable<Project>, ITags, IProperties
    {
        [PartitionKey]
        public string Tenant { get; set; }

        [UniqueKey]
        public string Name { get; set; }

        public ProjectType Type { get; set; }

        public ProjectIdentity Identity { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public AzureKeyVault KeyVault { get; set; }

        [DatabaseIgnore]
        public IList<User> Users { get; set; } = new List<User>();

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public bool Equals(Project other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Project);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.Ordinal);
    }
}
