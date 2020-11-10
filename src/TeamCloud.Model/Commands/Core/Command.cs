/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands.Core
{
    public abstract class Command<TPayload, TCommandResult> : ICommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected Command(CommandAction action, User user, TPayload payload = default, Guid? commandId = default)
        {
            CommandAction = action;
            User = user ?? throw new ArgumentNullException(nameof(user));
            Payload = payload;
            CommandId = commandId.GetValueOrDefault(Guid.NewGuid());

            if (payload is IOrganizationChild child)
                OrganizationId = child.Organization;
        }

        public Guid CommandId { get; private set; }

        public string OrganizationId { get; private set; }

        public CommandAction CommandAction { get; private set; }

        private string projectId;

        public virtual string ProjectId
        {
            get => Payload is Project project && !string.IsNullOrEmpty(project.Id) ? project.Id : projectId;
            protected set => projectId = value;
        }

        public User User { get; set; }

        public TPayload Payload
        {
            get => ((ICommand)this).Payload as TPayload;
            set => ((ICommand)this).Payload = value;
        }

        object ICommand.Payload { get; set; }

        public TCommandResult CreateResult()
        {
            var result = Activator.CreateInstance<TCommandResult>();

            result.CommandId = CommandId;
            result.OrganizationId = OrganizationId;
            result.CommandAction = CommandAction;
            result.RuntimeStatus = CommandRuntimeStatus.Unknown;

            return result;
        }

        ICommandResult ICommand.CreateResult()
            => CreateResult();
    }
}
