/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestration.Auditing.Model
{
    public static class CommandAuditExtensions
    {
        public static CommandAuditEntity Augment(this CommandAuditEntity entity, ICommand command, ICommandResult commandResult = default)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var timestamp = DateTime.UtcNow;

            entity.CommandId = command.CommandId.ToString();
            entity.Command = command.GetType().Name;

            entity.Project ??= command.Payload is Project project ? project.Name : null;
            entity.Created ??= timestamp;

            if (commandResult != null)
            {
                entity.Status = commandResult.RuntimeStatus;

                if (command is IProviderCommand && !commandResult.RuntimeStatus.IsUnknown())
                {
                    entity.Sent ??= timestamp;
                }

                if (commandResult.RuntimeStatus.IsFinal())
                {
                    entity.Processed ??= timestamp;
                }
                else if (command is IProviderCommand && commandResult.RuntimeStatus.IsActive())
                {
                    entity.Timeout ??= timestamp + commandResult.Timeout;
                }
            }

            return entity;
        }

    }
}
