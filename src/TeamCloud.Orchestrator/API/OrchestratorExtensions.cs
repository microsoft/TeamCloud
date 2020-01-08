/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Model;

namespace TeamCloud.Orchestrator
{
    public static class OrchestratorExtensions
    {
        public static ICommandResult<TResult> GetResult<TResult>(this DurableOrchestrationStatus orchestrationStatus)
            where TResult : new()
        {
            var result = new CommandResult<TResult>(Guid.Parse(orchestrationStatus.InstanceId))
            {
                CreatedTime = orchestrationStatus.CreatedTime,
                LastUpdatedTime = orchestrationStatus.LastUpdatedTime,
                RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus,
                CustomStatus = orchestrationStatus.CustomStatus?.ToString(),
            };

            if (orchestrationStatus.Output?.HasValues ?? false)
            {
                result.Result = orchestrationStatus.Output.ToObject<TResult>();
            }

            return result;
        }


        public static ICommandResult GetResult(this DurableOrchestrationStatus orchestrationStatus)
        {
            var result = new CommandResult(Guid.Parse(orchestrationStatus.InstanceId))
            {
                CreatedTime = orchestrationStatus.CreatedTime,
                LastUpdatedTime = orchestrationStatus.LastUpdatedTime,
                RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus,
                CustomStatus = orchestrationStatus.CustomStatus?.ToString(),
            };

            //if (orchestrationStatus.Output?.HasValues ?? false)
            //{
            //    result.Result = orchestrationStatus.Output.ToObject<TResult>();
            //}

            return result;
        }
    }
}