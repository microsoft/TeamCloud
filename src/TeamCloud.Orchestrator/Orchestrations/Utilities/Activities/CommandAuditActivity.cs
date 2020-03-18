/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using TeamCloud.Model.Auditing;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public static class CommandAuditActivity
    {
        [FunctionName(nameof(CommandAuditActivity))]
        public static async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            [Table("AuditCommands")] CloudTable commandTable,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (commandTable is null)
                throw new ArgumentNullException(nameof(commandTable));

            var (instanceId, provider, command, commandResult) =
                functionContext.GetInput<(string, Provider, ICommand, ICommandResult)>();

            try
            {
                var entity = new CommandAuditEntity()
                {
                    InstanceId = instanceId,
                    CommandId = command.CommandId.ToString()
                };

                var entityResult = await commandTable
                    .ExecuteAsync(TableOperation.Retrieve<CommandAuditEntity>(entity.TableEntity.PartitionKey, entity.TableEntity.RowKey))
                    .ConfigureAwait(false);

                entity = entityResult.HttpStatusCode == (int)HttpStatusCode.OK
                    ? (CommandAuditEntity)entityResult.Result
                    : entity;

                AugmentEntity(entity, provider, command, commandResult);

                await commandTable
                    .ExecuteAsync(TableOperation.InsertOrReplace(entity))
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log.LogWarning(exc, $"Failed to audit command {command?.GetType().Name ?? "UNKNOWN"} ({command?.CommandId ?? Guid.Empty}) for provider {provider?.Id ?? "UNKNOWN"}");
            }
        }

        private static void AugmentEntity(CommandAuditEntity entity, Provider provider, ICommand command, ICommandResult commandResult)
        {
            var timestamp = DateTime.UtcNow;

            entity.Provider = provider.Id;
            entity.Command = command.GetType().Name;
            entity.Project ??= command.ProjectId?.ToString();
            entity.Created ??= timestamp;

            if (commandResult != null)
            {
                entity.Status = commandResult.RuntimeStatus;

                if (commandResult.RuntimeStatus.IsFinal())
                {
                    entity.Sent ??= timestamp;
                    entity.Processed ??= timestamp;

                    if (commandResult.RuntimeStatus == CommandRuntimeStatus.Failed)
                    {
                        // the command ran into an error - trace the error in our audit log
                        entity.Errors = commandResult.Errors.Select(err => err.Message).ToArray();
                    }
                }
                else if (commandResult.RuntimeStatus.IsActive())
                {
                    // the provider returned an active state
                    // so we could set the sent state and 
                    // tace the timeout returned by the provider

                    entity.Sent ??= timestamp;
                    entity.Timeout ??= timestamp + commandResult.Timeout;
                }
            }
        }
    }
}
