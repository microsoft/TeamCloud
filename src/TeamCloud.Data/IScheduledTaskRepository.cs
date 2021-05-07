/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */
 
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IScheduledTaskRepository : IDocumentRepository<ScheduledTask>
    {
        Task RemoveAllAsync(string projectId);

        Task RemoveAsync(string projectId, string id);

        IAsyncEnumerable<ScheduledTask> ListAsync(string projectId, DayOfWeek day, int hour, int minute, int interval);
    }
}
