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

        public static void EnsureProjectMembership(this User user, Guid projectId, ProjectUserRole role, IDictionary<string, string> properties = null)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var membership = user.ProjectMembership(projectId);

            if (membership is null)
                user.ProjectMemberships.Add(new ProjectMembership
                {
                    ProjectId = projectId,
                    Role = role,
                    Properties = properties ?? new Dictionary<string, string>()
                });
            else
            {
                membership.Role = role;
                if (properties != null)
                    membership.MergeProperties(properties, overwriteExistingValues: true);
            }
        }

        public static bool HasEqualMemberships(this User user, User other)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (other is null) throw new ArgumentNullException(nameof(other));

            return user.ProjectMemberships.SequenceEqual(other.ProjectMemberships, new ProjectMembershipComparer());
        }
    }
}
