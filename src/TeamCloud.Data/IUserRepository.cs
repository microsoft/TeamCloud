/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IUserRepository
    {
        Task<User> GetAsync(string organization, string id);

        IAsyncEnumerable<User> ListAsync(string organization);

        IAsyncEnumerable<User> ListAsync(string organization, string projectId);

        IAsyncEnumerable<User> ListOwnersAsync(string organization, string projectId);

        IAsyncEnumerable<User> ListAdminsAsync(string organization);

        IAsyncEnumerable<string> ListOrgsAsync(User user);

        IAsyncEnumerable<string> ListOrgsAsync(string userId);

        Task<User> AddAsync(User user);

        Task<User> SetAsync(User user);

        Task<User> RemoveAsync(User user);

        Task RemoveProjectMembershipsAsync(string organization, string projectId);

        Task<User> RemoveProjectMembershipAsync(User user, string projectId);

        Task<User> AddProjectMembershipAsync(User user, ProjectMembership membership);

        Task<User> AddProjectMembershipAsync(User user, string projectId, ProjectUserRole role, IDictionary<string, string> properties);

        Task<User> SetOrganizationInfoAsync(User user);
    }
}
