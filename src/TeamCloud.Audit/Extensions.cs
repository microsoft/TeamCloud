/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TeamCloud.Audit.Model;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Audit
{
    public static class Extensions
    {
        private const string COMMAND_QUALIFIER = "command";
        private const string RESULT_QUALIFIER = "result";

        public static IServiceCollection AddTeamCloudAudit(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .TryAddSingleton<ICommandAuditWriter, CommandAuditWriter>();

            serviceCollection
                .TryAddSingleton<ICommandAuditReader, CommandAuditReader>();

            return serviceCollection;
        }

        private static string ToPathSegmentSafe(string id)
            => Guid.TryParse(id, out var idParsed) ? idParsed.ToString() : Guid.Empty.ToString();

        internal static string GetPath(this ICommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return $"{ToPathSegmentSafe(command.OrganizationId)}/{ToPathSegmentSafe(command.ProjectId)}/{command.CommandId}.{COMMAND_QUALIFIER}.json";
        }

        internal static string GetPath(this ICommandResult commandResult)
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            return $"{ToPathSegmentSafe(commandResult.OrganizationId)}/{ToPathSegmentSafe(commandResult.ProjectId)}/{commandResult.CommandId}.{RESULT_QUALIFIER}.json";
        }

        internal static string GetCommandPath(this CommandAuditEntity commandAuditEntity)
        {
            if (commandAuditEntity is null)
                throw new ArgumentNullException(nameof(commandAuditEntity));

            return $"{ToPathSegmentSafe(commandAuditEntity.OrganizationId)}/{ToPathSegmentSafe(commandAuditEntity.ProjectId)}/{commandAuditEntity.CommandId}.{COMMAND_QUALIFIER}.json";
        }

        internal static string GetResultPath(this CommandAuditEntity commandAuditEntity)
        {
            if (commandAuditEntity is null)
                throw new ArgumentNullException(nameof(commandAuditEntity));

            return $"{ToPathSegmentSafe(commandAuditEntity.OrganizationId)}/{ToPathSegmentSafe(commandAuditEntity.ProjectId)}/{commandAuditEntity.CommandId}.{RESULT_QUALIFIER}.json";
        }

        internal static async Task<CloudBlobContainer> EnsureContainerAsync(this Lazy<CloudBlobContainer> cloudBlobContainer)
        {
            if (!cloudBlobContainer.IsValueCreated)
            {
                _ = await cloudBlobContainer.Value
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);
            }

            return cloudBlobContainer.Value;
        }

        internal static async Task<CloudTable> EnsureTableAsync(this Lazy<CloudTable> cloudTable)
        {
            if (!cloudTable.IsValueCreated)
            {
                _ = await cloudTable.Value
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);
            }

            return cloudTable.Value;
        }
    }
}
