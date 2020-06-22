/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Orchestration.Auditing
{
    public static class CommandAuditExtension
    {
        public static Task AuditAsync(this IDurableOrchestrationContext functionContext, ICommand command, ICommandResult commandResult = default, IProvider provider = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return functionContext.CallActivityWithRetryAsync(nameof(CommandAuditActivity), (command, commandResult, provider));
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
                sanitizedHubName += TaskHubPadding;

            return sanitizedHubName;
        }
    }
}
