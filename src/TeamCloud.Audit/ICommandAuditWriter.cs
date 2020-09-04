/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Audit
{
    public interface ICommandAuditWriter
    {
        Task AuditAsync(ICommand command, ICommandResult commandResult = default, string providerId = default);
    }
}
