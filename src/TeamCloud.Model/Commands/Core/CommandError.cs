using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Model.Commands.Core
{
    public sealed class CommandError
    {
        public string Message { get; set; }

        public CommandErrorSeverity Severity { get; set; } = CommandErrorSeverity.Error;
    }
}
