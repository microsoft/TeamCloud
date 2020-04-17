/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
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
    }
}
