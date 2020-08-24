/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface ITeamCloudRepository
    {
        Task<TeamCloudInstanceDocument> GetAsync();

        Task<TeamCloudInstanceDocument> SetAsync(TeamCloudInstanceDocument teamCloudInstance);
    }
}
