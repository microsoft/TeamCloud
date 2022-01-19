/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Commands.Core;
public static class CommandErrorExtensions
{
    public static void Add(this IList<CommandError> errors, Exception exception, CommandErrorSeverity severity = CommandErrorSeverity.Error)
    {
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));

        if (exception is null)
            throw new ArgumentNullException(nameof(exception));

        if (exception is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                errors.Add(innerException, severity);

        }
        else
        {
            errors.Add(new CommandError()
            {
                Message = exception.Message,
                Severity = severity
            });
        }
    }

    public static Exception ToException(this CommandError error)
    {
        if (error is null)
            throw new ArgumentNullException(nameof(error));

        return new Exception(error.Message)
        {
            Source = TeamCloudSerialize.SerializeObject(error)
        };
    }

    public static Exception ToException(this IEnumerable<CommandError> errors, CommandErrorSeverity minSeverity = CommandErrorSeverity.Error)
    {
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));

        var affectedErrors = errors
            .Where(error => error.Severity >= minSeverity);

        if (affectedErrors.Skip(1).Any())
            return new AggregateException(affectedErrors.Select(error => error.ToException()));

        return affectedErrors.SingleOrDefault()?.ToException();
    }
}
