/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.Audit.Model;

namespace TeamCloud.Audit
{
    public interface ICommandAuditReader
    {
        Task<CommandAuditEntity> GetAsync(Guid organizationId, Guid commandId, bool includeJsonDumps = false);

        IAsyncEnumerable<CommandAuditEntity> ListAsync(Guid organizationId, Guid? projectId = null, TimeSpan? timeRange = null, [FromQuery] string[]? commands = null);
    }
}
