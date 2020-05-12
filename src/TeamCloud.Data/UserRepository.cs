/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IUsersRepositoryReadOnly
    {
        Task<User> GetAsync(Guid id);

        IAsyncEnumerable<User> ListAsync();

        IAsyncEnumerable<User> ListAsync(Guid projectId);

        IAsyncEnumerable<User> ListAdminsAsync();
    }

    public interface IUsersRepository : IUsersRepositoryReadOnly
    {
        Task<User> AddAsync(User user);

        Task<User> SetAsync(User user);

        Task<User> RemoveAsync(User user);
    }
}
