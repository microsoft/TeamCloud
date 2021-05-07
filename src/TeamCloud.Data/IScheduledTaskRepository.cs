/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IScheduleRepository : IDocumentRepository<Schedule>
    {
        Task RemoveAllAsync(string projectId);

        Task RemoveAsync(string projectId, string id);

        IAsyncEnumerable<Schedule> ListAsync(string projectId, DayOfWeek day, int hour, int minute, int interval);
    }
}
