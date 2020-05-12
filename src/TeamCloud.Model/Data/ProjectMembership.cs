/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class ProjectMembership
    {
        public Guid ProjectId { get; set; }

        public ProjectUserRole Role { get; set; }

        public bool Equals(ProjectMembership other) => ProjectId.Equals(other?.ProjectId);

        public override bool Equals(object obj)
            => base.Equals(obj) || this.Equals(obj as ProjectMembership);

        public override int GetHashCode()
            => this.ProjectId.GetHashCode();
    }

    public class ProjectMembershipComparer : IEqualityComparer<ProjectMembership>
    {
        public bool Equals(ProjectMembership x, ProjectMembership y)
        {
            if (ReferenceEquals(x, y))
                return true;
            else if (x == null || y == null)
                return false;
            else if (x.ProjectId == y.ProjectId)
                return true;
            else
                return false;
        }

        public int GetHashCode(ProjectMembership obj)
            => (obj ?? throw new ArgumentNullException(nameof(obj))).ProjectId.GetHashCode();
    }
}
