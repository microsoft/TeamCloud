/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Model.Commands.Core
{
    public sealed class CommandError
    {
        public string Message { get; set; }

        public CommandErrorSeverity Severity { get; set; } = CommandErrorSeverity.Error;
    }
}
