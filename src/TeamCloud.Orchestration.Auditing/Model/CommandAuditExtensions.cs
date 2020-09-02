/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestration.Auditing.Model
{
    public static class CommandAuditExtensions
    {
        internal static CommandAuditEntity Augment(this CommandAuditEntity entity, ICommand command, ICommandResult commandResult, string providerId)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var timestamp = DateTime.UtcNow;

            entity.CommandId = command.CommandId.ToString();
            entity.Command = command.GetType().Name;
            entity.Event = (command as ProviderEventCommand)?.Payload?.EventType;
            entity.Provider ??= providerId;
            entity.Created ??= timestamp;

            if (commandResult != null)
            {
                entity.Status = commandResult.RuntimeStatus;

                if (!commandResult.RuntimeStatus.IsUnknown())
                {
                    if (commandResult.RuntimeStatus.IsFinal())
                    {
                        entity.Processed ??= timestamp;
                    }
                    else if (command is IProviderCommand)
                    {
                        entity.Sent ??= timestamp;
                        entity.Timeout ??= timestamp + commandResult.Timeout;
                    }
                }
            }

            return entity;
        }
    }
}
