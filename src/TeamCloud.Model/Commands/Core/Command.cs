/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Serialization;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Commands.Core
{
    [JsonConverter(typeof(CommandConverter))]
    public interface ICommand : IValidatable
    {
        Guid CommandId { get; }

        string ProjectId { get; }

        object User { get; set; }

        ICommandResult CreateResult();

        object Payload { get; set; }
    }

    public interface ICommand<TUser, TPayload> : ICommand
        where TUser : IUser, new()
        where TPayload : new()
    {
        new TUser User { get; set; }

        new TPayload Payload { get; set; }
    }

    public interface ICommand<TUser, TPayload, TCommandResult> : ICommand<TUser, TPayload>
        where TUser : class, IUser, new()
        where TPayload : class, new()
        where TCommandResult : ICommandResult
    {
        new TCommandResult CreateResult();
    }


    public abstract class Command<TUser, TPayload, TCommandResult> : ICommand<TUser, TPayload, TCommandResult>
        where TUser : class, IUser, new()
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        protected Command(TUser user, TPayload payload = default, Guid? commandId = default)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Payload = payload;
            CommandId = commandId.GetValueOrDefault(Guid.NewGuid());
        }

        public Guid CommandId { get; private set; }

        private string projectId = default;

        public virtual string ProjectId
        {
            get => Payload is IProject project && !string.IsNullOrEmpty(project.Id) ? project.Id : projectId;
            protected set => projectId = value;
        }

        public TUser User
        {
            get => ((ICommand)this).User as TUser;
            set => ((ICommand)this).User = value;
        }

        object ICommand.User { get; set; }


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
            result.RuntimeStatus = CommandRuntimeStatus.Unknown;

            return result;
        }

        ICommandResult ICommand.CreateResult()
            => CreateResult();
    }
}
