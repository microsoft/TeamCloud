/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Serialization;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands.Core
{
    [JsonConverter(typeof(CommandConverter))]
    public interface ICommand : IValidatable
    {
        Guid CommandId { get; }

        Guid? ProjectId { get; }

        User User { get; set; }

        ICommandResult CreateResult();

        object Payload { get; set; }
    }

    public interface ICommand<TPayload> : ICommand
        where TPayload : new()
    {
        new TPayload Payload { get; set; }
    }

    public interface ICommand<TPayload, TCommandResult> : ICommand<TPayload>
        where TPayload : class, new()
        where TCommandResult : ICommandResult
    {
        new TCommandResult CreateResult();
    }


    public abstract class Command<TPayload, TCommandResult> : ICommand<TPayload, TCommandResult>
        where TPayload : class, new()
        where TCommandResult : ICommandResult, new()
    {
        public Guid CommandId { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public virtual Guid? ProjectId { get; set; }

        public User User { get; set; }

        public TPayload Payload
        {
            get => ((ICommand)this).Payload as TPayload;
            set => ((ICommand)this).Payload = value;
        }

        object ICommand.Payload { get; set; }

        protected Command(User user, TPayload payload)
        {
            User = user;
            Payload = payload;
        }

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
