/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectMembership : IProperties
    {
        public string ProjectId { get; set; }

        public ProjectUserRole Role { get; set; }

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    public class ProjectMembershipComparer : IEqualityComparer<ProjectMembership>
    {
        public bool Equals(ProjectMembership x, ProjectMembership y)
        {
            if (ReferenceEquals(x, y)
            || (x != null && y != null
               && x.ProjectId == y.ProjectId
               && x.Role == y.Role
               && x.Properties.SequenceEqual(y.Properties)))
                return true;
            else
                return false;
        }

        public int GetHashCode(ProjectMembership obj)
            => (obj ?? throw new ArgumentNullException(nameof(obj))).GetHashCode();
    }
}
