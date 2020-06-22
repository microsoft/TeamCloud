/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Data
{
    public interface ITeamCloudRepository
    {
        Task<TeamCloudInstance> GetAsync();

        Task<TeamCloudInstance> SetAsync(TeamCloudInstance teamCloudInstance);
    }
}
