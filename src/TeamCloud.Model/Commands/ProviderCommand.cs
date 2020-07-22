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
        string ProviderId { get; set; }

        string SystemDataApi { get; }

        string ProjectDataApi { get; }

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
        protected ProviderCommand(Uri api, User user, TPayload payload, Guid? commandId = default) : base(api, user, payload, commandId)
        { }

        public string ProviderId { get; set; }

        public string SystemDataApi => Api is null || string.IsNullOrEmpty(ProviderId) ? null : new Uri(Api, $"api/providers/{ProviderId}").ToString();

        public string ProjectDataApi => Api is null || string.IsNullOrEmpty(ProviderId) || string.IsNullOrEmpty(ProjectId) ? null : new Uri(Api, $"api/projects/{ProjectId}/providers/{ProviderId}").ToString();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, IDictionary<string, string>> Results { get; set; } = new Dictionary<string, IDictionary<string, string>>();
    }
}
