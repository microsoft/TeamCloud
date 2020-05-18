/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamCloud.Model.Data
{
    public static class UserExtensions
    {
        public static bool IsAdmin(this User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.Role == TeamCloudUserRole.Admin;
        }

        public static bool IsCreator(this User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.Role == TeamCloudUserRole.Creator;
        }

        public static bool IsAdminOrCreator(this User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.IsAdmin() || user.IsCreator();
        }

        public static bool IsOwner(this User user, Guid projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.RoleFor(projectId) == ProjectUserRole.Owner;
        }

        public static bool IsMember(this User user, Guid projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var role = user.RoleFor(projectId);

            return role == ProjectUserRole.Owner || role == ProjectUserRole.Member;
        }

        public static ProjectUserRole RoleFor(this User user, Guid projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.ProjectMembership(projectId)?.Role ?? ProjectUserRole.None;
        }

        public static ProjectMembership ProjectMembership(this User user, Guid projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.ProjectMemberships.FirstOrDefault(m => m.ProjectId == projectId);
        }

        public static void EnsureProjectMembership(this User user, ProjectMembership membership)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (membership is null) throw new ArgumentNullException(nameof(membership));

            var existingMembership = user.ProjectMemberships.FirstOrDefault(m => m.ProjectId == membership.ProjectId);

            if (existingMembership is null)
                user.ProjectMemberships.Add(membership);
            else
            {
                existingMembership.Role = membership.Role;
                existingMembership.MergeProperties(membership.Properties, overwriteExistingValues: true);
            }
        }

        public static void EnsureProjectMembership(this User user, Guid projectId, ProjectUserRole role, IDictionary<string, string> properties = null)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            user.EnsureProjectMembership(new ProjectMembership
            {
                ProjectId = projectId,
                Role = role,
                Properties = properties ?? new Dictionary<string, string>()
            });
        }

        public static bool HasEqualMemberships(this User user, User other)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (other is null) throw new ArgumentNullException(nameof(other));

            return user.ProjectMemberships.SequenceEqual(other.ProjectMemberships, new ProjectMembershipComparer());
        }

        public static bool HasEqualMembership(this User user, User other, Guid projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (other is null) throw new ArgumentNullException(nameof(other));

            return new ProjectMembershipComparer().Equals(user.ProjectMembership(projectId), other.ProjectMembership(projectId));
        }

        public static bool HasEqualMembership(this User user, ProjectMembership membership)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (membership is null) throw new ArgumentNullException(nameof(membership));

            return new ProjectMembershipComparer().Equals(user.ProjectMembership(membership.ProjectId), membership);
        }

        public static void EnsureTeamCloudInfo(this User user, TeamCloudUserRole role, IDictionary<string, string> properties = null)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            user.Role = role;
            if (properties != null)
                user.MergeProperties(properties, overwriteExistingValues: true);
        }

        public static bool HasEqualTeamCloudInfo(this User user, User other)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (other is null) throw new ArgumentNullException(nameof(other));

            return user.Role == other.Role
                && user.Properties.SequenceEqual(other.Properties);
        }
    }
}
