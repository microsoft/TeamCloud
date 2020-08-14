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
        protected ProviderCommand(User user, TPayload payload, Guid? commandId = default)
            : base(user, payload, commandId)
        { }

        private ProviderCommandLinks links = null;

        public ProviderCommandLinks Links
        {
            get => links ??= new ProviderCommandLinks(this);
            set => links = value?.SetContext(this) ?? throw new ArgumentNullException(nameof(Links));
        }

        public string ProviderId { get; set; }

        public IDictionary<string, string> Properties { get; set; }
            = new Dictionary<string, string>();

        public IDictionary<string, IDictionary<string, string>> Results { get; set; }
            = new Dictionary<string, IDictionary<string, string>>();
    }
}
