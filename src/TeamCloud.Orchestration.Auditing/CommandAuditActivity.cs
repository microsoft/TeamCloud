/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration.Auditing.Model;

namespace TeamCloud.Orchestration.Auditing
{
    public static class CommandAuditActivity
    {
        [FunctionName(nameof(CommandAuditActivity))]
        public static Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            [DurableClient] IDurableClient durableClient,
            IBinder binder,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (binder is null)
                throw new ArgumentNullException(nameof(binder));

            var (provider, command, commandResult) =
                functionContext.GetInput<(Provider, ICommand, ICommandResult)>();

            try
            {
                var prefix = durableClient.GetTaskHubName(true);

                return Task.WhenAll
                (
                   WriteAuditTableAsync(binder, prefix, provider, command, commandResult),
                   WriteAuditContainerAsync(binder, prefix, provider, command, commandResult)
                );
            }
            catch (Exception exc)
            {
                log?.LogWarning(exc, $"Failed to audit command {command?.GetType().Name ?? "UNKNOWN"} ({command?.CommandId ?? Guid.Empty}) for provider {provider?.Id ?? "UNKNOWN"}");

                return Task.CompletedTask;
            }
        }

        private static async Task WriteAuditTableAsync(IBinder binder, string prefix, Provider provider, ICommand command, ICommandResult commandResult)
        {
            var entity = new CommandAuditEntity(command, provider);

            var auditTable = await binder
                .BindAsync<CloudTable>(new TableAttribute($"{prefix}Audit"))
                .ConfigureAwait(false);

            var entityResult = await auditTable
                .ExecuteAsync(TableOperation.Retrieve<CommandAuditEntity>(entity.TableEntity.PartitionKey, entity.TableEntity.RowKey))
                .ConfigureAwait(false);

            entity = entityResult.HttpStatusCode == (int)HttpStatusCode.OK
                ? (CommandAuditEntity)entityResult.Result
                : entity;

            await auditTable
                .ExecuteAsync(TableOperation.InsertOrReplace(entity.Augment(command, commandResult)))
                .ConfigureAwait(false);
        }

        private static async Task WriteAuditContainerAsync(IBinder binder, string prefix, Provider provider, ICommand command, ICommandResult commandResult)
        {
            var tasks = new List<Task>()
            {
                WriteBlobAsync(command.CommandId, command)
            };

            if (commandResult != null)
            {
                tasks.Add(WriteBlobAsync(command.CommandId, commandResult));
            }

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);

            async Task WriteBlobAsync(Guid commandId, object data)
            {
#pragma warning disable CA1308 // Normalize strings to uppercase

                var auditBlob = await binder
                    .BindAsync<CloudBlockBlob>(new BlobAttribute($"{prefix.ToLowerInvariant()}-audit/{commandId}/{provider?.Id ?? "orchestrator"}/{data.GetType().Name}.json"))
                    .ConfigureAwait(false);

                await auditBlob
                    .UploadTextAsync(JsonConvert.SerializeObject(data, Formatting.Indented))
                    .ConfigureAwait(false);

#pragma warning restore CA1308 // Normalize strings to uppercase
            }
        }
    }
}
