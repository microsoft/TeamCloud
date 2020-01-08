/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;

namespace TeamCloud.Model
{
    [JsonConverter(typeof(CommandConverter))]
    public interface ICommand
    {
        Guid? ProjectId { get; }

        Guid UserId { get; set; }
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
        public Command(TPayload payload)
        {
            Payload = payload;
        }

        public TPayload Payload { get; set; }

        [JsonIgnore]
        public virtual Guid? ProjectId { get; set; }

        public Guid UserId { get; set; }
    }
}