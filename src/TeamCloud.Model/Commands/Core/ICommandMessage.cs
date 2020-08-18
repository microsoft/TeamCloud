/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Commands.Serialization;
using TeamCloud.Model.Common;

namespace TeamCloud.Model.Commands.Core
{
    [JsonConverter(typeof(CommandMessageConverter))]
    public interface ICommandMessage : IValidatable
    {
        ICommand Command { get; }

        [JsonIgnore]
        Guid? CommandId { get; }

        [JsonIgnore]
        Type CommandType { get; }
    }
}
