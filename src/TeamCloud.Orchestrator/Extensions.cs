/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
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
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Providers;

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

        internal static ICommandResult CreateResult(this ICommand command, DurableOrchestrationStatus orchestrationStatus)
        {
            var result = (orchestrationStatus.Output?.HasValues ?? false) ? orchestrationStatus.Output.ToObject<ICommandResult>() : command.CreateResult();

            result.CreatedTime = orchestrationStatus.CreatedTime;
            result.LastUpdatedTime = orchestrationStatus.LastUpdatedTime;
            result.RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus;
            result.CustomStatus = orchestrationStatus.CustomStatus?.ToString();

            return result;
        }

        internal static ICommandResult GetCommandResult(this DurableOrchestrationStatus orchestrationStatus)
        {
            if (orchestrationStatus.Output?.HasValues ?? false)
            {
                var result = orchestrationStatus.Output.ToObject<ICommandResult>();

                result.CreatedTime = orchestrationStatus.CreatedTime;
                result.LastUpdatedTime = orchestrationStatus.LastUpdatedTime;
                result.RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus;
                result.CustomStatus = orchestrationStatus.CustomStatus?.ToString();

                return result;
            }
            else if (orchestrationStatus.Input?.HasValues ?? false)
            {
                var command = orchestrationStatus.Input.ToObject<OrchestratorCommandMessage>()?.Command;

                return command?.CreateResult(orchestrationStatus);
            }

            return null;
        }

        internal static IEnumerable<Task<ICommandResult>> GetProviderCommandTasks(this List<Provider> providers, ICommand command, IDurableOrchestrationContext functionContext)
            => providers.Select(provider => functionContext.CallSubOrchestratorAsync<ICommandResult>(nameof(ProviderCommandOrchestration), (provider, command.GetProviderCommand(provider))));

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
