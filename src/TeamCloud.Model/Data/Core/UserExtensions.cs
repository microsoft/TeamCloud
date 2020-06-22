/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamCloud.Model.Data.Core
{
    public static class UserExtensions
    {
        public static bool IsAdmin(this IUser user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.Role == TeamCloudUserRole.Admin;
        }

        public static bool IsCreator(this IUser user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.Role == TeamCloudUserRole.Creator;
        }

        public static bool IsAdminOrCreator(this IUser user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            return user.IsAdmin() || user.IsCreator();
        }

        public static bool IsOwner(this IUser user, string projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (projectId is null) throw new ArgumentNullException(nameof(projectId));

            return user.RoleFor(projectId) == ProjectUserRole.Owner;
        }

        public static bool IsMember(this IUser user, string projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (projectId is null) throw new ArgumentNullException(nameof(projectId));

            var role = user.RoleFor(projectId);

            return role == ProjectUserRole.Owner || role == ProjectUserRole.Member;
        }

        public static ProjectUserRole RoleFor(this IUser user, string projectId)
            => user?.ProjectMembership(projectId)?.Role ?? ProjectUserRole.None;

        public static ProjectMembership ProjectMembership(this IUser user, string projectId)
            => user?.ProjectMemberships.FirstOrDefault(m => m.ProjectId == projectId);

        public static IDictionary<string, string> ProjectProperties(this IUser user, string projectId, bool overwriteExistingValues = true)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (projectId is null) throw new ArgumentNullException(nameof(projectId));

            if (user.Properties is null)
                user.Properties = new Dictionary<string, string>();

            var properties = user.Properties;

            if (overwriteExistingValues)
            {
                user.ProjectMembership(projectId)
                    .Properties
                    .ToList()
                    .ForEach(t => properties[t.Key] = t.Value);
            }
            else
            {
                var keyValuePairs = user.ProjectMembership(projectId)
                    .Properties
                    .Concat(properties);

                properties = keyValuePairs
                    .GroupBy(kvp => kvp.Key)
                    .Where(kvp => kvp.First().Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
            }

            return properties;
        }

        public static void EnsureProjectMembership(this IUser user, ProjectMembership membership)
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

        public static void EnsureProjectMembership(this IUser user, string projectId, ProjectUserRole role, IDictionary<string, string> properties = null)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (projectId is null) throw new ArgumentNullException(nameof(projectId));

            user.EnsureProjectMembership(new ProjectMembership
            {
                ProjectId = projectId,
                Role = role,
                Properties = properties ?? new Dictionary<string, string>()
            });
        }

        public static bool HasEqualMemberships(this IUser user, IUser other)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (other is null) throw new ArgumentNullException(nameof(other));

            return user.ProjectMemberships.SequenceEqual(other.ProjectMemberships, new ProjectMembershipComparer());
        }

        public static bool HasEqualMembership(this IUser user, IUser other, string projectId)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (other is null) throw new ArgumentNullException(nameof(other));
            if (projectId is null) throw new ArgumentNullException(nameof(projectId));

            return new ProjectMembershipComparer().Equals(user.ProjectMembership(projectId), other.ProjectMembership(projectId));
        }

        public static bool HasEqualMembership(this IUser user, ProjectMembership membership)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (membership is null) throw new ArgumentNullException(nameof(membership));

            return new ProjectMembershipComparer().Equals(user.ProjectMembership(membership.ProjectId), membership);
        }

        public static void EnsureTeamCloudInfo(this IUser user, TeamCloudUserRole role, IDictionary<string, string> properties = null)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            user.Role = role;
            if (properties != null)
                user.MergeProperties(properties, overwriteExistingValues: true);
        }

        public static bool HasEqualTeamCloudInfo(this IUser user, IUser other)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (other is null) throw new ArgumentNullException(nameof(other));

            return user.Role == other.Role
                && user.Properties.SequenceEqual(other.Properties);
        }
    }
}
