/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Providers
{
    public class ProviderCommand
    {
        public ICommand Command { get; set; }

        public Provider Provider { get; set; }

        public string CallbackUrl { get; set; }
    }

    public class ProviderCommandResult : ProviderCommand
    {
        public ICommandResult CommandResult { get; set; }

        public string Error { get; set; }

        public bool Succeeded => string.IsNullOrEmpty(Error);
    }
}
