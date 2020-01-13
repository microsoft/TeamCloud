/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

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
