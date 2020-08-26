/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Commands
{
    public interface IProviderCommand
        : ICommand, IReferenceLinksAccessor<IProviderCommand, ProviderCommandReferenceLinks>
    {
        string ProviderId { get; set; }

        IDictionary<string, string> Properties { get; set; }

        IDictionary<string, IDictionary<string, string>> Results { get; set; }
    }

    public interface IProviderCommand<TPayload>
        : ICommand<User, TPayload>, IProviderCommand
        where TPayload : new()
    { }

    public interface IProviderCommand<TPayload, TCommandResult>
        : ICommand<User, TPayload, TCommandResult>, IProviderCommand<TPayload>
        where TPayload : class, new()
        where TCommandResult : ICommandResult
    { }
}
