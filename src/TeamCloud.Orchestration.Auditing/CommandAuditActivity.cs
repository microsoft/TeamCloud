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
using TeamCloud.Orchestration.Auditing.Model;

namespace TeamCloud.Orchestration.Auditing
{
    public static class CommandAuditActivity
    {
        [FunctionName(nameof(CommandAuditActivity))]
        public static async Task RunActivity(
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

            var (command, commandResult, providerId) =
                functionContext.GetInput<(ICommand, ICommandResult, string)>();

            try
            {
                var prefix = durableClient.GetTaskHubName(true);

                await Task.WhenAll
                (
                   WriteAuditTableAsync(binder, prefix, command, commandResult, providerId),
                   WriteAuditContainerAsync(binder, prefix, command, commandResult, providerId)
                )
                .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log?.LogWarning(exc, $"Failed to audit command {command?.GetType().Name ?? "UNKNOWN"} ({command?.CommandId ?? Guid.Empty}) for provider {providerId ?? "UNKNOWN"}");
            }
        }

        private static async Task WriteAuditTableAsync(IBinder binder, string prefix, ICommand command, ICommandResult commandResult, string providerId)
        {
            var entity = new CommandAuditEntity(command, providerId);

            var auditTable = await binder
                .BindAsync<CloudTable>(new TableAttribute($"{prefix}Audit"))
                .ConfigureAwait(false);

            var entityResult = await auditTable
                .ExecuteAsync(TableOperation.Retrieve<CommandAuditEntity>(entity.TableEntity.PartitionKey, entity.TableEntity.RowKey))
                .ConfigureAwait(false);

            if (entityResult.HttpStatusCode == (int)HttpStatusCode.OK)
                entity = (entityResult.Result as CommandAuditEntity) ?? entity;

            await auditTable
                .ExecuteAsync(TableOperation.InsertOrReplace(entity.Augment(command, commandResult, providerId)))
                .ConfigureAwait(false);
        }

        private static async Task WriteAuditContainerAsync(IBinder binder, string prefix, ICommand command, ICommandResult commandResult, string providerId)
        {
            var tasks = new List<Task>()
            {
                WriteBlobAsync(command)
            };

            if (commandResult != null)
            {
                tasks.Add(WriteBlobAsync(commandResult));
            }

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);

            async Task WriteBlobAsync(object data)
            {
#pragma warning disable CA1308 // Normalize strings to uppercase

                var auditPath = $"{prefix.ToLowerInvariant()}-audit/{command.ProjectId}/{command.CommandId}/{providerId}/{data.GetType().Name}.json";

                var auditBlob = await binder
                    .BindAsync<CloudBlockBlob>(new BlobAttribute(auditPath.Replace("//", "/", StringComparison.OrdinalIgnoreCase)))
                    .ConfigureAwait(false);

                await auditBlob
                    .UploadTextAsync(JsonConvert.SerializeObject(data, Formatting.Indented))
                    .ConfigureAwait(false);

#pragma warning restore CA1308 // Normalize strings to uppercase
            }
        }
    }
}
