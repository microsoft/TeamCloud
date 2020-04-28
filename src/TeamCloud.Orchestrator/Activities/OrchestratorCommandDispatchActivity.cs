/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Activities
{
    public static class OrchestratorCommandDispatchActivity
    {
        [FunctionName(nameof(OrchestratorCommandDispatchActivity))]
        public static string RunActivity([ActivityTrigger] ICommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return command switch
            {
                _ => $"{command.GetType().Name}Orchestration"
            };
        }
    }
}
