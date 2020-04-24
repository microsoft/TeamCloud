/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestration.Auditing
{
    public static class CommandAuditExtension
    {
        public static Task AuditAsync(this IDurableOrchestrationContext functionContext, ICommand command, ICommandResult commandResult = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (command is IProviderCommand providerCommand)
                throw new ArgumentException($"Missing provider information for command", nameof(command));

            return functionContext.CallActivityWithRetryAsync(nameof(CommandAuditActivity), (default(Provider), command, commandResult));
        }

        public static Task AuditAsync(this IDurableOrchestrationContext functionContext, IProviderCommand command, Provider provider, ICommandResult commandResult = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            return functionContext.CallActivityWithRetryAsync(nameof(CommandAuditActivity), (provider, command, commandResult));
        }

        internal static string GetTaskHubName(this IDurableClient client, bool sanitized)
        {
            const int MaxTaskHubNameSize = 45;
            const int MinTaskHubNameSize = 3;
            const string TaskHubPadding = "Hub";

            if (client is null)
                throw new ArgumentNullException(nameof(client));

            if (!sanitized)
                return client.TaskHubName;

            var validHubNameCharacters = client.TaskHubName
                    .ToCharArray()
                    .Where(char.IsLetterOrDigit);

            if (!validHubNameCharacters.Any())
                return "DefaultTaskHub";

            if (char.IsNumber(validHubNameCharacters.First()))
            {
                // Azure Table storage requires that the task hub does not start 
                // with a number. If it does, prepend "t" to the beginning.

                validHubNameCharacters = validHubNameCharacters.ToList();
                ((List<char>)validHubNameCharacters).Insert(0, 't');
            }

            var sanitizedHubName = new string(validHubNameCharacters
                .Take(MaxTaskHubNameSize)
                .ToArray());

            if (sanitizedHubName.Length < MinTaskHubNameSize)
                sanitizedHubName = sanitizedHubName + TaskHubPadding;

            return sanitizedHubName;
        }
    }
}
