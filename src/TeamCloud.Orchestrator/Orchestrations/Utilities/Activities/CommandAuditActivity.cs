/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
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
                    .ExecuteAsync(TableOperation.Retrieve<CommandAuditEntity>(entity.PartitionKey, entity.RowKey))
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
            entity.Provider = provider.Id;
            entity.Command = command.GetType().Name;
            entity.Created ??= DateTime.UtcNow;

            if (commandResult != null)
            {
                entity.Status = commandResult.RuntimeStatus.ToString();

                if (commandResult.RuntimeStatus.IsFinal())
                {
                    entity.Processed ??= DateTime.UtcNow;
                }
            }
        }
    }
}
