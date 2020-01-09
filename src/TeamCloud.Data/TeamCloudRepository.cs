/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using TeamCloud.Model;

namespace TeamCloud.Data
{
    public interface ITeamCloudRepositoryReadOnly
    {
        Task<TeamCloudInstance> GetAsync();
    }

    public interface ITeamCloudRepository : ITeamCloudRepositoryReadOnly
    {
        Task<TeamCloudInstance> SetAsync(TeamCloudInstance teamCloudInstance);
    }
}
