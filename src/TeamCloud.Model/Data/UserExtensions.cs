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

            return user.RoleFor(projectId) == ProjectUserRole.Member;
        }

        public static bool IsOwnerOrMember(this User user, Guid projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.IsOwner(projectId) || user.IsMember(projectId);
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

        public static void EnsureProjectMembership(this User user, Guid projectId, ProjectUserRole role)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var membership = user.ProjectMembership(projectId);

            if (membership is null)
                user.ProjectMemberships.Add(new ProjectMembership { ProjectId = projectId, Role = role });
            else
                membership.Role = role;
        }
    }
}
