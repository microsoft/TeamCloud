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
    public abstract class ProviderCommand<TPayload, TCommandResult>
        : Command<User, TPayload, TCommandResult>, IProviderCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected ProviderCommand(CommandAction action, User user, TPayload payload, Guid? commandId = default)
            : base(action, user, payload, commandId)
        { }

        private ProviderCommandReferenceLinks links = null;

        public ProviderCommandReferenceLinks Links
        {
            get => links ??= new ProviderCommandReferenceLinks(this);
            set => links = value?.SetContext(this) ?? throw new ArgumentNullException(nameof(Links));
        }

        public string ProviderId { get; set; }

        public IDictionary<string, string> Properties { get; set; }
            = new Dictionary<string, string>();

        public IDictionary<string, IDictionary<string, string>> Results { get; set; }
            = new Dictionary<string, IDictionary<string, string>>();
    }

    public abstract class ProviderCreateCommand<TPayload, TCommandResult> : ProviderCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected ProviderCreateCommand(User user, TPayload payload, Guid? commandId = default)
            : base(CommandAction.Create, user, payload, commandId)
        { }
    }

    public abstract class ProviderUpdateCommand<TPayload, TCommandResult> : ProviderCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected ProviderUpdateCommand(User user, TPayload payload, Guid? commandId = default)
            : base(CommandAction.Update, user, payload, commandId)
        { }
    }

    public abstract class ProviderDeleteCommand<TPayload, TCommandResult> : ProviderCommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected ProviderDeleteCommand(User user, TPayload payload, Guid? commandId = default)
            : base(CommandAction.Delete, user, payload, commandId)
        { }
    }
}
