/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model
{
    public static class Extensions
    {
        public static IDisposable BeginCommandScope(this ILogger logger, ICommand command, Provider provider = default)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return logger.BeginScope(new Dictionary<string, object>()
            {
                { "commandId", command.CommandId },
                { "commandType", command.GetType().Name },
                { "projectId", command.ProjectId },
                { "providerId", provider?.Id }
            });
        }

        public static Exception GetException(this ICommandResult commandResult)
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (commandResult.Errors?.Skip(1).Any() ?? false)
                return new AggregateException(commandResult.Errors);

            if (commandResult.Errors?.Any() ?? false)
                return commandResult.Errors.Single();

            return null;
        }

        public static Exception GetException(this IEnumerable<ICommandResult> commandResults)
        {
            if (commandResults is null)
                throw new ArgumentNullException(nameof(commandResults));

            var exceptions = commandResults
                .Where(cr => cr.Errors.Any())
                .Select(cr => cr.GetException());

            if (exceptions.Skip(1).Any())
                return new AggregateException(exceptions);

            if (exceptions.Any())
                return exceptions.Single();

            return null;
        }

    }
}
