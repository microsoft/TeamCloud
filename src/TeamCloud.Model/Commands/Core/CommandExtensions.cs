/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;

namespace TeamCloud.Model.Commands.Core
{
    public static class CommandExtensions
    {
        public static string StatusUrl(this ICommandResult result)
            => (result ?? throw new ArgumentNullException(nameof(result))).Links.TryGetValue("status", out var statusUrl) ? statusUrl : null;

        public static string GetTypeName(this ICommand command, bool prettyPrint = false)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return prettyPrint && command.GetType().IsGenericType
                ? PrettyPrintTypeName(command.GetType())
                : command.GetType().Name;
        }

        public static string GetTypeName(this ICommandResult commandResult, bool prettyPrint = false)
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            return prettyPrint && commandResult.GetType().IsGenericType
                ? PrettyPrintTypeName(commandResult.GetType())
                : commandResult.GetType().Name;
        }

        private static string PrettyPrintTypeName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var typename = type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.OrdinalIgnoreCase));
            return $"{typename}<{string.Join(", ", type.GetGenericArguments().Select(PrettyPrintTypeName))}>";
        }
    }
}
