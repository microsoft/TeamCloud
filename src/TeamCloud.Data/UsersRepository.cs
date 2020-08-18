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
        Task<UserDocument> GetAsync(string id);

        IAsyncEnumerable<UserDocument> ListAsync();

        IAsyncEnumerable<UserDocument> ListAsync(string projectId);

        IAsyncEnumerable<UserDocument> ListOwnersAsync(string projectId);

        IAsyncEnumerable<UserDocument> ListAdminsAsync();

        Task<UserDocument> AddAsync(UserDocument user);

        Task<UserDocument> SetAsync(UserDocument user);

        Task<UserDocument> RemoveAsync(UserDocument user);

        Task RemoveProjectMembershipsAsync(string projectId);

        Task<UserDocument> RemoveProjectMembershipAsync(UserDocument user, string projectId);

        Task<UserDocument> AddProjectMembershipAsync(UserDocument user, ProjectMembership membership);

        Task<UserDocument> AddProjectMembershipAsync(UserDocument user, string projectId, ProjectUserRole role, IDictionary<string, string> properties);

        Task<UserDocument> SetTeamCloudInfoAsync(UserDocument user);
    }
}
