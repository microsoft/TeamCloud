/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IUsersRepository
    {
        Task<User> GetAsync(string id);

        IAsyncEnumerable<User> ListAsync();

        IAsyncEnumerable<User> ListAsync(string projectId);

        IAsyncEnumerable<User> ListOwnersAsync(string projectId);

        IAsyncEnumerable<User> ListAdminsAsync();

        Task<User> AddAsync(User user);

        Task<User> SetAsync(User user);

        Task<User> RemoveAsync(User user);

        Task RemoveProjectMembershipsAsync(string projectId);

        Task<User> RemoveProjectMembershipAsync(User user, string projectId);

        Task<User> AddProjectMembershipAsync(User user, ProjectMembership membership);

        Task<User> AddProjectMembershipAsync(User user, string projectId, ProjectUserRole role, IDictionary<string, string> properties);

        Task<User> SetTeamCloudInfoAsync(User user);
    }
}
