/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{

    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class Project : ReferenceLinksAccessor<Project, ProjectReferenceLinks>, IProject<User>, IEquatable<Project>
    {
        public string Id { get; set; }
            = Guid.NewGuid().ToString();

        public string Name { get; set; }

        public ProjectType Type { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public IList<User> Users { get; set; }
            = new List<User>();


        public IDictionary<string, string> Tags { get; set; }
            = new Dictionary<string, string>();

        public IDictionary<string, string> Properties { get; set; }
            = new Dictionary<string, string>();

        public bool Equals(Project other)
            => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Project);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
