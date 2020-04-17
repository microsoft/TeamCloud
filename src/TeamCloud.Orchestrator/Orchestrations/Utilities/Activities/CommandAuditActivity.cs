/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using TeamCloud.Model.Auditing;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public static class CommandAuditActivity
    {
        private static string DefaultProviderName = Assembly.GetCallingAssembly().GetName().Name;

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

            var (provider, command, commandResult) =
                functionContext.GetInput<(Provider, ICommand, ICommandResult)>();

            try
            {
                var entity = new CommandAuditEntity()
                {
                    CommandId = command.CommandId.ToString(),
                    ProviderId = command is IProviderCommand ? provider.Id : DefaultProviderName
                };

                var entityResult = await commandTable
                    .ExecuteAsync(TableOperation.Retrieve<CommandAuditEntity>(entity.TableEntity.PartitionKey, entity.TableEntity.RowKey))
                    .ConfigureAwait(false);

                entity = entityResult.HttpStatusCode == (int)HttpStatusCode.OK
                    ? (CommandAuditEntity)entityResult.Result
                    : entity;

                AugmentEntity(entity, command, commandResult);

                await commandTable
                    .ExecuteAsync(TableOperation.InsertOrReplace(entity))
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log.LogWarning(exc, $"Failed to audit command {command?.GetType().Name ?? "UNKNOWN"} ({command?.CommandId ?? Guid.Empty}) for provider {provider?.Id ?? "UNKNOWN"}");
            }
        }

        private static void AugmentEntity(CommandAuditEntity entity, ICommand command, ICommandResult commandResult)
        {
            var timestamp = DateTime.UtcNow;

            entity.Command = command.GetType().Name;
            entity.ProjectId ??= command.ProjectId?.ToString();
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

                    if (commandResult.RuntimeStatus == CommandRuntimeStatus.Failed)
                    {
                        // the command ran into an error - trace the error in our audit log
                        entity.Errors = commandResult.Errors.Select(err => err.Message).ToArray();
                    }
                }
                else if (command is IProviderCommand && commandResult.RuntimeStatus.IsActive())
                {
                    // the provider returned an active state
                    // so we could set the sent state and 
                    // tace the timeout returned by the provider

                    entity.Timeout ??= timestamp + commandResult.Timeout;
                }
            }
        }
    }
}
