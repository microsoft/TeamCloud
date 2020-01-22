/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Commands;
using TeamCloud.Orchestrator.Orchestrations.Commands;

namespace TeamCloud.Orchestrator
{
    internal static class Extensions
    {
        private static readonly PropertyInfo IsDevStoreAccountProperty = typeof(CloudStorageAccount).GetProperty("IsDevStoreAccount", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly int[] FinalRuntimeStatus = new int[]
        {
            (int) OrchestrationRuntimeStatus.Canceled,
            (int) OrchestrationRuntimeStatus.Completed,
            (int) OrchestrationRuntimeStatus.Terminated
        };

        internal static bool IsFinalRuntimeStatus(this DurableOrchestrationStatus status)
        {
            if (status is null) throw new ArgumentNullException(nameof(status));

            return FinalRuntimeStatus.Contains((int)status.RuntimeStatus);
        }

        internal static Task WaitForProjectCommandsAsync(this IDurableOrchestrationContext context, ICommand command)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            if (command is null) throw new ArgumentNullException(nameof(command));

            if (command.ProjectId.HasValue)
                return context.CallSubOrchestratorAsync(nameof(ProjectCommandSerialization.ProjectCommandSerializationOrchestrator), command);
            else
                return Task.CompletedTask;
        }

        internal static ICommandResult<TResult> GetResult<TResult>(this DurableOrchestrationStatus orchestrationStatus)
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

        internal static ICommandResult GetResult(this DurableOrchestrationStatus orchestrationStatus)
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

        internal static async Task<JObject> GetJObjectAsync(this Url url, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            var json = await url.GetJsonAsync(cancellationToken, completionOption).ConfigureAwait(false);

            return JObject.FromObject(json);
        }

        internal static async Task<JObject> GetJObjectAsync(this IFlurlRequest request, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            var json = await request.GetJsonAsync(cancellationToken, completionOption).ConfigureAwait(false);

            return JObject.FromObject(json);
        }

        internal static async Task<JObject> GetJObjectAsync(this string url, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            var json = await url.GetJsonAsync(cancellationToken, completionOption).ConfigureAwait(false);

            return JObject.FromObject(json);
        }

        internal static bool IsDevelopmentStorage(this CloudStorageAccount cloudStorageAccount)
            => (bool)IsDevStoreAccountProperty.GetValue(cloudStorageAccount);
    }
}
