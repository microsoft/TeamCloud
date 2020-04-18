/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Orchestrations.Utilities.Entities;

namespace TeamCloud.Orchestrator
{
    internal static class Extensions
    {
        internal static Task<IDisposable> LockAsync<TContainerDocument>(this IDurableOrchestrationContext functionContext, string containerDocumentId)
            where TContainerDocument : class, IContainerDocument
            => functionContext.LockAsync(functionContext.GetEntityId<TContainerDocument>(containerDocumentId));

        internal static Task<IDisposable> LockAsync<TContainerDocument>(this IDurableOrchestrationContext functionContext, TContainerDocument containerDocument)
            where TContainerDocument : class, IContainerDocument
            => functionContext.LockAsync(functionContext.GetEntityId<TContainerDocument>(containerDocument?.Id));

        internal static bool IsLockedBy<TContainerDocument>(this IDurableOrchestrationContext functionContext, string containerDocumentId)
            where TContainerDocument : class, IContainerDocument
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId<TContainerDocument>(containerDocumentId));

        internal static bool IsLockedBy<TContainerDocument>(this IDurableOrchestrationContext functionContext, TContainerDocument containerDocument)
            where TContainerDocument : class, IContainerDocument
            => functionContext.IsLocked(out var locks) && locks.Contains(functionContext.GetEntityId<TContainerDocument>(containerDocument?.Id));


        private static EntityId GetEntityId<TContainerDocument>(this IDurableOrchestrationContext functionContext, string containerDocumentId)
            where TContainerDocument : class, IContainerDocument
        {
            if (string.IsNullOrWhiteSpace(containerDocumentId))
                throw new ArgumentException("A container document id must not NULL, EMPTY, or WHITESPACE", nameof(containerDocumentId));

            if (Guid.TryParse(containerDocumentId, out var containerDocumentGuid) && containerDocumentGuid == Guid.Empty)
                throw new ArgumentException("A container document id must not an empty GUID", nameof(containerDocumentId));

            return new EntityId(nameof(DocumentEntityLock), $"{containerDocumentId}@{typeof(TContainerDocument)}");
        }

        internal static ICommandResult CreateResult(this ICommand command, DurableOrchestrationStatus orchestrationStatus)
        {
            if (orchestrationStatus is null)
                throw new ArgumentNullException(nameof(orchestrationStatus));

            var result = (orchestrationStatus.Output?.HasValues ?? false) ? orchestrationStatus.Output.ToObject<ICommandResult>() : command.CreateResult();

            return result.ApplyStatus(orchestrationStatus);
        }

        internal static ICommandResult ApplyStatus(this ICommandResult commandResult, DurableOrchestrationStatus orchestrationStatus)
        {
            if (orchestrationStatus is null)
                throw new ArgumentNullException(nameof(orchestrationStatus));

            commandResult.CreatedTime = orchestrationStatus.CreatedTime;
            commandResult.LastUpdatedTime = orchestrationStatus.LastUpdatedTime;
            commandResult.RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus;
            commandResult.CustomStatus = orchestrationStatus.CustomStatus?.ToString();

            return commandResult;
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

        internal static IDictionary<string, string> Override(this IDictionary<string, string> instance, IDictionary<string, string> mainOverride)
        {
            var keyValuePairs = instance
                .Concat(mainOverride);

            return keyValuePairs
                .GroupBy(kvp => kvp.Key)
                .Where(kvp => kvp.Last().Value != null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Last().Value);
        }

        private static readonly Regex DynamicPropertyExpression = new Regex(@"^\[(\$\..*)\]$", RegexOptions.Compiled);

        internal static IDictionary<string, string> Resolve(this IDictionary<string, string> instance, object context, bool removeNull = false)
            => context is null ? instance.Resolve(removeNull) : instance.Resolve(() => JObject.FromObject(context), removeNull);

        internal static IDictionary<string, string> Resolve(this IDictionary<string, string> instance, bool removeNull = false)
            => instance.Resolve(() => JObject.Parse("{}"), removeNull);

        private static IDictionary<string, string> Resolve(this IDictionary<string, string> instance, Func<JObject> jsonCallback, bool removeNull = false)
        {
            var contextJson = default(JObject);
            var resolvedProperties = new Dictionary<string, string>();

            foreach (var item in instance)
            {
                var itemValue = item.Value;

                if (!string.IsNullOrWhiteSpace(itemValue))
                {
                    var match = DynamicPropertyExpression.Match(itemValue);

                    if (match.Success)
                    {
                        contextJson ??= jsonCallback?.Invoke() ?? default;

                        itemValue = contextJson?.HasValues ?? false
                            ? contextJson.SelectToken(match.Groups[1].Value)?.ToString()
                            : default;

                        Debug.WriteLine($"Resolved '{item.Key}': {item.Value} => {itemValue ?? "NULL"}");
                    }
                }

                if (!(itemValue != null) || !removeNull)
                {
                    resolvedProperties.Add(item.Key, itemValue);
                }
                else
                {
                    Debug.WriteLine($"Resolved '{item.Key}': removed as value was resolved as NULL");
                }
            }

            return resolvedProperties;
        }

        internal static DateTime NextHour(this DateTime dateTime)
             => dateTime.Date.AddHours(dateTime.Hour + 1);
    }
}
