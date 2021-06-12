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
            CommandId = commandId.GetValueOrDefault(Guid.NewGuid());
            CommandAction = action;
            User = user ?? throw new ArgumentNullException(nameof(user));
            Payload = payload;
        }

        public Guid CommandId { get; private set; }

        public Guid ParentId { get; set; } = Guid.Empty;

        public CommandAction CommandAction { get; private set; }

        private string organizationId;

        public string OrganizationId
        {
            get => (Payload as IOrganizationContext)?.Organization ?? (Payload as Organization)?.Id ?? organizationId;
            set => organizationId = value;
        }

        private string projectId;

        public virtual string ProjectId
        {
            get => (Payload as IProjectContext)?.ProjectId ?? (Payload as Project)?.Id ?? projectId;
            set => projectId = value;
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
            result.ProjectId = ProjectId;
            result.CommandAction = CommandAction;
            result.RuntimeStatus = CommandRuntimeStatus.Unknown;

            return result;
        }

        ICommandResult ICommand.CreateResult()
            => CreateResult();
    }

    public abstract class CreateCommand<TPayload, TCommandResult> : Command<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected CreateCommand(User user, TPayload payload, Guid? commandId = default) : base(CommandAction.Create, user, payload, commandId)
        { }
    }

    public abstract class UpdateCommand<TPayload, TCommandResult> : Command<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected UpdateCommand(User user, TPayload payload, Guid? commandId = default) : base(CommandAction.Update, user, payload, commandId)
        { }
    }

    public abstract class DeleteCommand<TPayload, TCommandResult> : Command<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected DeleteCommand(User user, TPayload payload, Guid? commandId = default) : base(CommandAction.Delete, user, payload, commandId)
        { }
    }

    public abstract class CustomCommand<TPayload, TCommandResult> : Command<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected CustomCommand(User user, TPayload payload, Guid? commandId = default) : base(CommandAction.Custom, user, payload, commandId)
        { }
    }
}
