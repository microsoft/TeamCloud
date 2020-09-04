/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator
{
    internal static class GlobalExtensions
    {


        internal static ICommandResult CreateResult(this ICommand command, DurableOrchestrationStatus orchestrationStatus)
        {
            if (orchestrationStatus is null)
                throw new ArgumentNullException(nameof(orchestrationStatus));

            var result = (orchestrationStatus.Output?.HasValues ?? false) ? orchestrationStatus.Output.ToObject<ICommandResult>() : command.CreateResult();

            return result.ApplyStatus(orchestrationStatus);
        }

        internal static ICommandResult ApplyStatus(this ICommandResult commandResult, DurableOrchestrationStatus orchestrationStatus)
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (orchestrationStatus is null)
                throw new ArgumentNullException(nameof(orchestrationStatus));

            commandResult.CreatedTime = GetNullWhenMinValue(orchestrationStatus.CreatedTime);
            commandResult.LastUpdatedTime = GetNullWhenMinValue(orchestrationStatus.LastUpdatedTime);
            commandResult.RuntimeStatus = (CommandRuntimeStatus)orchestrationStatus.RuntimeStatus;
            commandResult.CustomStatus = orchestrationStatus.CustomStatus?.ToString();

            return commandResult;

            static DateTime? GetNullWhenMinValue(DateTime dateTime)
                => (dateTime == DateTime.MinValue ? default(DateTime?) : dateTime);
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
    }
}
