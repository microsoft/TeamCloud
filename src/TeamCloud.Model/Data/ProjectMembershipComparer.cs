/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamCloud.Model.Data
{
    public class ProjectMembershipComparer : IEqualityComparer<ProjectMembership>
    {
        public bool Equals(ProjectMembership x, ProjectMembership y)
        {
            if (ReferenceEquals(x, y)
            || x != null && y != null
               && x.ProjectId == y.ProjectId
               && x.Role == y.Role
               && x.Properties.SequenceEqual(y.Properties))
                return true;
            else
                return false;
        }

        public int GetHashCode(ProjectMembership obj)
            => (obj ?? throw new ArgumentNullException(nameof(obj))).GetHashCode();
    }
}
