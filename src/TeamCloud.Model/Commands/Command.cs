/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Serialization;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    [JsonConverter(typeof(CommandConverter))]
    public interface ICommand
    {
        Guid CommandId { get; }

        Guid? ProjectId { get; }

        User User { get; set; }
    }


    public interface ICommand<TPayload, TResult> : ICommand
        where TPayload : new()
        where TResult : new()
    {
        TPayload Payload { get; set; }
    }


    public abstract class Command<TPayload, TResult> : ICommand<TPayload, TResult>
        where TPayload : new()
        where TResult : new()
    {
        public Guid CommandId { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public virtual Guid? ProjectId { get; set; }

        public User User { get; set; }

        public TPayload Payload { get; set; }

        public Command(User user, TPayload payload)
        {
            User = user;
            Payload = payload;
        }
    }
}