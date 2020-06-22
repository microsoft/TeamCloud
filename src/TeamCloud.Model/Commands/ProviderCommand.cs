/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public interface IProviderCommand : ICommand
    {
        IDictionary<string, string> Properties { get; set; }

        IDictionary<string, IDictionary<string, string>> Results { get; set; }
    }

    public interface IProviderCommand<TPayload> : ICommand<User, TPayload>, IProviderCommand
        where TPayload : new()
    { }

    public interface IProviderCommand<TPayload, TCommandResult> : ICommand<User, TPayload, TCommandResult>, IProviderCommand<TPayload>
        where TPayload : class, new()
        where TCommandResult : ICommandResult
    { }

    public abstract class ProviderCommand<TPayload, TCommandResult> : Command<User, TPayload, TCommandResult>, IProviderCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected ProviderCommand(User user, TPayload payload, Guid? commandId = default) : base(user, payload, commandId)
        { }

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, IDictionary<string, string>> Results { get; set; } = new Dictionary<string, IDictionary<string, string>>();
    }
}
